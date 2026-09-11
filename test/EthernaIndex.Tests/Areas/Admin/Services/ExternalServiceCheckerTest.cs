// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
//
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.EthernaIndex.Services.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.EthernaIndex.Areas.Admin.Services
{
    public class ExternalServiceCheckerTest
    {
        // Classes.
        private sealed class StubHttpMessageHandler(
            IDictionary<string, (HttpStatusCode StatusCode, string Body)> responsesByPath)
            : HttpMessageHandler
        {
            // Properties.
            public List<string> RequestedPaths { get; } = [];

            // Methods.
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var path = request.RequestUri!.AbsolutePath.TrimStart('/');
                RequestedPaths.Add(path);

                if (!responsesByPath.TryGetValue(path, out var response))
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("no stub for " + path)
                    });

                return Task.FromResult(new HttpResponseMessage(response.StatusCode)
                {
                    Content = new StringContent(response.Body, System.Text.Encoding.UTF8, "application/json")
                });
            }
        }

        // Consts.
        private const string CreditPath = "api/v0.3/users/current/credit2";
        private const string DiscoveryPath = ".well-known/openid-configuration";
        private const string GatewayUrl = "https://gateway.test";
        private const string HealthPath = "health";
        private const string SsoUrl = "https://sso.test";
        private const string TokenPath = "connect/token";

        // Tests.
        [Fact]
        public async Task CheckAllReportsEveryServiceAsReachableAndAuthenticated()
        {
            // Arrange.
            using var handler = BuildHandler(new Dictionary<string, (HttpStatusCode, string)>
            {
                [DiscoveryPath] = (HttpStatusCode.OK, """{"issuer":"https://sso.test"}"""),
                [TokenPath] = (HttpStatusCode.OK, """{"access_token":"t","expires_in":3600}"""),
                [HealthPath] = (HttpStatusCode.OK, """{"status":"ok","version":"0.4.11.0"}"""),
                [CreditPath] = (HttpStatusCode.OK, """{"isUnlimited":false,"balance":"10100000000000000000"}""")
            });
            var checker = BuildChecker(handler);

            // Action.
            var statuses = (await checker.CheckAllAsync()).ToArray();

            // Assert.
            var sso = statuses.Single(s => s.Name == "Etherna SSO");
            Assert.Equal(SsoUrl, sso.Address);
            Assert.True(sso.Reachability.Succeeded);
            Assert.Equal("issuer https://sso.test", sso.Reachability.Detail);
            Assert.True(sso.Authentication.Succeeded);
            Assert.Equal("token issued, expires in 3600 s", sso.Authentication.Detail);

            var gateway = statuses.Single(s => s.Name == "Etherna Gateway");
            Assert.Equal(GatewayUrl, gateway.Address);
            Assert.True(gateway.Reachability.Succeeded);
            Assert.Equal("version 0.4.11.0", gateway.Reachability.Detail);
            Assert.True(gateway.Authentication.Succeeded);
            Assert.Equal("authenticated, credit 10.1 xDai", gateway.Authentication.Detail);
        }

        [Fact]
        public async Task CheckAllReportsTheStatusAndBodyOfAFailingService()
        {
            // Arrange.
            using var handler = BuildHandler(new Dictionary<string, (HttpStatusCode, string)>
            {
                [DiscoveryPath] = (HttpStatusCode.OK, """{"issuer":"https://sso.test"}"""),
                [TokenPath] = (HttpStatusCode.OK, """{"access_token":"t","expires_in":3600}"""),
                [HealthPath] = (HttpStatusCode.ServiceUnavailable, "node is not ready"),
                [CreditPath] = (HttpStatusCode.OK, """{"isUnlimited":true}""")
            });
            var checker = BuildChecker(handler);

            // Action.
            var gateway = (await checker.CheckAllAsync()).Single(s => s.Name == "Etherna Gateway");

            // Assert.
            Assert.False(gateway.Reachability.Succeeded);
            Assert.Contains("503", gateway.Reachability.Error, StringComparison.Ordinal);
            Assert.Contains("node is not ready", gateway.Reachability.Error, StringComparison.Ordinal);
            //an unreachable service is still checked for authentication: the two answers are independent
            Assert.True(gateway.Authentication.Succeeded);
            Assert.Equal("authenticated, unlimited credit", gateway.Authentication.Detail);
        }

        [Fact]
        public async Task CheckAllExplainsAnAddressThatDoesNotServeTheGatewayApi()
        {
            // Arrange.
            using var handler = BuildHandler(new Dictionary<string, (HttpStatusCode, string)>
            {
                [DiscoveryPath] = (HttpStatusCode.OK, """{"issuer":"https://sso.test"}"""),
                [TokenPath] = (HttpStatusCode.OK, """{"access_token":"t","expires_in":3600}"""),
                [HealthPath] = (HttpStatusCode.OK, """{"status":"ok"}""")
                //no credit endpoint: a bee node answers 404 there
            });
            var checker = BuildChecker(handler);

            // Action.
            var gateway = (await checker.CheckAllAsync()).Single(s => s.Name == "Etherna Gateway");

            // Assert.
            Assert.True(gateway.Reachability.Succeeded);
            Assert.False(gateway.Authentication.Succeeded);
            Assert.Contains("bee node", gateway.Authentication.Error, StringComparison.Ordinal);
        }

        [Fact]
        public async Task CheckAllReportsMissingClientCredentialsWithoutCallingTheTokenEndpoint()
        {
            // Arrange.
            using var handler = BuildHandler(new Dictionary<string, (HttpStatusCode, string)>
            {
                [DiscoveryPath] = (HttpStatusCode.OK, """{"issuer":"https://sso.test"}"""),
                [TokenPath] = (HttpStatusCode.OK, """{"access_token":"t","expires_in":3600}"""),
                [HealthPath] = (HttpStatusCode.OK, """{"status":"ok"}"""),
                [CreditPath] = (HttpStatusCode.OK, """{"isUnlimited":true}""")
            });
            var checker = BuildChecker(handler, withClientCredentials: false);

            // Action.
            var sso = (await checker.CheckAllAsync()).Single(s => s.Name == "Etherna SSO");

            // Assert.
            Assert.True(sso.Reachability.Succeeded);
            Assert.False(sso.Authentication.Succeeded);
            Assert.Contains("SsoServer:Clients:Services:ClientId", sso.Authentication.Error, StringComparison.Ordinal);
            Assert.DoesNotContain(TokenPath, handler.RequestedPaths);
        }

        [Fact]
        public async Task CheckAllReportsAnUnconfiguredGatewayWithoutCallingIt()
        {
            // Arrange.
            using var handler = BuildHandler(new Dictionary<string, (HttpStatusCode, string)>
            {
                [DiscoveryPath] = (HttpStatusCode.OK, """{"issuer":"https://sso.test"}"""),
                [TokenPath] = (HttpStatusCode.OK, """{"access_token":"t","expires_in":3600}""")
            });
            var checker = BuildChecker(handler, gatewayUrl: "");

            // Action.
            var gateway = (await checker.CheckAllAsync()).Single(s => s.Name == "Etherna Gateway");

            // Assert.
            Assert.Equal("(not configured)", gateway.Address);
            Assert.False(gateway.Reachability.Succeeded);
            Assert.Contains("Swarm:GatewayUrl", gateway.Reachability.Error, StringComparison.Ordinal);
            Assert.DoesNotContain(HealthPath, handler.RequestedPaths);
        }

        // Helpers.
        private static ExternalServiceChecker BuildChecker(
            StubHttpMessageHandler handler,
            bool withClientCredentials = true,
            string gatewayUrl = GatewayUrl)
        {
            Dictionary<string, string?> settings = new() { ["SsoServer:BaseUrl"] = SsoUrl };
            if (withClientCredentials)
            {
                settings["SsoServer:Clients:Services:ClientId"] = "clientId";
                settings["SsoServer:Clients:Services:Secret"] = "clientSecret";
            }
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(() => new HttpClient(handler, disposeHandler: false));

            return new ExternalServiceChecker(
                configuration,
                httpClientFactoryMock.Object,
                Options.Create(new SwarmOptions { GatewayUrl = gatewayUrl }));
        }

        private static StubHttpMessageHandler BuildHandler(
            Dictionary<string, (HttpStatusCode StatusCode, string Body)> responsesByPath) =>
            new(responsesByPath);
    }
}
