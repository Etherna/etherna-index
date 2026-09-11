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

using Etherna.Authentication;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Services.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Services
{
    internal sealed class ExternalServiceChecker(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IOptions<SwarmOptions> swarmOptions)
        : IExternalServiceChecker
    {
        // Consts.
        private const string GatewayCreditPath = "api/v0.3/users/current/credit2";
        private const string GatewayHealthPath = "health";
        private const int MaxErrorLength = 300;
        private const string SsoClientIdConfigKey = "SsoServer:Clients:Services:ClientId";
        private const string SsoClientSecretConfigKey = "SsoServer:Clients:Services:Secret";
        private const string SsoDiscoveryPath = ".well-known/openid-configuration";
        private const string SsoTokenPath = "connect/token";
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(10);
        private const decimal WeiPerXDai = 1_000_000_000_000_000_000m;

        // Methods.
        public async Task<IEnumerable<ExternalServiceStatus>> CheckAllAsync(CancellationToken cancellationToken = default)
        {
            var statuses = await Task.WhenAll(
                CheckSsoServerAsync(cancellationToken),
                CheckEthernaGatewayAsync(cancellationToken));
            return statuses;
        }

        // Helpers.
        private async Task<ExternalServiceStatus> CheckSsoServerAsync(CancellationToken cancellationToken)
        {
            var baseUrl = configuration["SsoServer:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                return MissingConfigurationStatusHelper("Etherna SSO", "SsoServer:BaseUrl");

            // The discovery document is public: it tells whether the server answers, and which issuer it declares.
            var reachability = await ProbeAsync(
                $"GET {SsoDiscoveryPath}",
                async token =>
                {
                    var httpClient = httpClientFactory.CreateClient();
                    using var response = await httpClient.GetAsync(
                        BuildUrlHelper(baseUrl, SsoDiscoveryPath), token);
                    await EnsureSuccessHelper(response, token);

                    var issuer = (await ReadJsonHelper(response, token))
                        .TryGetProperty("issuer", out var issuerElement) ? issuerElement.GetString() : null;
                    return issuer is null ? "discovery document served" : $"issuer {issuer}";
                },
                cancellationToken);

            // Request a token with the client credentials application used to download from the gateway.
            // It is asked to the server every time, so the check tells the state now and not that of a cached token.
            var clientId = configuration[SsoClientIdConfigKey];
            var clientSecret = configuration[SsoClientSecretConfigKey];
            var authentication = string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret)
                ? new ExternalServiceProbe(
                    $"POST {SsoTokenPath}",
                    TimeSpan.Zero,
                    error: $"{SsoClientIdConfigKey} or {SsoClientSecretConfigKey} is not configured")
                : await ProbeAsync(
                    $"POST {SsoTokenPath}",
                    async token =>
                    {
                        var httpClient = httpClientFactory.CreateClient();
                        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            ["client_id"] = clientId,
                            ["client_secret"] = clientSecret,
                            ["grant_type"] = "client_credentials",
                            ["scope"] = EthernaScopes.UserApiGateway
                        });
                        using var response = await httpClient.PostAsync(
                            BuildUrlHelper(baseUrl, SsoTokenPath), content, token);
                        await EnsureSuccessHelper(response, token);

                        var json = await ReadJsonHelper(response, token);
                        return json.TryGetProperty("expires_in", out var expiresIn)
                            ? $"token issued, expires in {expiresIn.GetInt32()} s"
                            : "token issued";
                    },
                    cancellationToken);

            return new ExternalServiceStatus("Etherna SSO", baseUrl, reachability, authentication);
        }

        private async Task<ExternalServiceStatus> CheckEthernaGatewayAsync(CancellationToken cancellationToken)
        {
            var baseUrl = swarmOptions.Value.GatewayUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
                return MissingConfigurationStatusHelper("Etherna Gateway", "Swarm:GatewayUrl");

            var reachability = await ProbeAsync(
                $"GET {GatewayHealthPath}",
                async token =>
                {
                    var httpClient = httpClientFactory.CreateClient();
                    using var response = await httpClient.GetAsync(
                        BuildUrlHelper(baseUrl, GatewayHealthPath), token);
                    await EnsureSuccessHelper(response, token);

                    var json = await ReadJsonHelper(response, token);
                    return json.TryGetProperty("version", out var version)
                        ? $"version {version.GetString()}"
                        : "healthy";
                },
                cancellationToken);

            // Read the credit of the account paying for the downloads, with the same authenticated client
            // the manifest reads use: it proves the token is accepted, and shows what is left to spend.
            var authentication = await ProbeAsync(
                $"GET {GatewayCreditPath}",
                async token =>
                {
                    var httpClient = httpClientFactory.CreateClient(CommonConsts.GatewayHttpClientName);
                    using var response = await httpClient.GetAsync(
                        BuildUrlHelper(baseUrl, GatewayCreditPath), token);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        throw new HttpRequestException(
                            "404 Not Found: the configured address doesn't expose the gateway api, is it a bee node?");
                    await EnsureSuccessHelper(response, token);

                    var json = await ReadJsonHelper(response, token);
                    if (json.TryGetProperty("isUnlimited", out var isUnlimited) && isUnlimited.GetBoolean())
                        return "authenticated, unlimited credit";
                    return json.TryGetProperty("balance", out var balance) &&
                           decimal.TryParse(balance.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var wei)
                        ? FormattableString.Invariant($"authenticated, credit {wei / WeiPerXDai:0.####} xDai")
                        : "authenticated";
                },
                cancellationToken);

            return new ExternalServiceStatus("Etherna Gateway", baseUrl, reachability, authentication);
        }

        private static Uri BuildUrlHelper(string baseUrl, string path) =>
            new(baseUrl.TrimEnd('/') + '/' + path);

        private static async Task EnsureSuccessHelper(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
                return;

            var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            throw new HttpRequestException(
                $"{(int)response.StatusCode} {response.ReasonPhrase}{(body.Length == 0 ? "" : ": " + body)}");
        }

        private static ExternalServiceStatus MissingConfigurationStatusHelper(string name, string configKey) =>
            new(name,
                "(not configured)",
                new ExternalServiceProbe("configuration", TimeSpan.Zero, error: $"{configKey} is not configured"),
                new ExternalServiceProbe("configuration", TimeSpan.Zero, error: $"{configKey} is not configured"));

        private static async Task<ExternalServiceProbe> ProbeAsync(
            string description,
            Func<CancellationToken, Task<string>> probeAsync,
            CancellationToken cancellationToken)
        {
            using var timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutTokenSource.CancelAfter(ProbeTimeout);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var detail = await probeAsync(timeoutTokenSource.Token);
                return new ExternalServiceProbe(description, stopwatch.Elapsed, detail);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new ExternalServiceProbe(
                    description,
                    stopwatch.Elapsed,
                    error: FormattableString.Invariant($"no answer within {ProbeTimeout.TotalSeconds:0} s"));
            }
            catch (Exception exception) when (
                exception is HttpRequestException or JsonException or InvalidOperationException or UriFormatException)
            {
                return new ExternalServiceProbe(description, stopwatch.Elapsed, error: TruncateHelper(exception.Message));
            }
        }

        private static async Task<JsonElement> ReadJsonHelper(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using (stream.ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync<JsonElement>(stream, cancellationToken: cancellationToken);
            }
        }

        private static string TruncateHelper(string message)
        {
            message = string.Join(' ', message.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()));
            return message.Length <= MaxErrorLength ? message : message[..MaxErrorLength] + "...";
        }
    }
}
