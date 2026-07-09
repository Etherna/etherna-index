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

using Etherna.DomainEvents;
using Etherna.DomainEvents.AspNetCore;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Infrastructure;
using Etherna.EthernaIndex.Services.Options;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.Sdk.Tools.Video.Services;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Etherna.EthernaIndex.Services
{
    public static class ServiceCollectionExtensions
    {
        // Consts.
        private const string EventHandlersSubNamespace = "EventHandlers";

        // Methods.
        /// <param name="gatewayHttpClientName">Name of the named <see cref="HttpClient"/>, configured in the host
        /// with a client-credentials bearer token, used by the <see cref="ISwarmClient"/> to authenticate Gateway downloads.</param>
        public static void AddDomainServices(
            this IServiceCollection services,
            IConfiguration configuration,
            string gatewayHttpClientName)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(gatewayHttpClientName);
            
            var currentType = typeof(ServiceCollectionExtensions).GetTypeInfo();
            var eventHandlersNamespace = $"{currentType.Namespace}.{EventHandlersSubNamespace}";

            // Events.
            //register handlers in Ioc
            var eventHandlerTypes = from t in typeof(ServiceCollectionExtensions).GetTypeInfo().Assembly.GetTypes()
                                    where t.IsClass && t.Namespace == eventHandlersNamespace
                                    where t.GetInterfaces().Contains(typeof(IEventHandler))
                                    select t;

            services.AddDomainEvents(eventHandlerTypes);
            
            // Options.
            services.Configure<SwarmOptions>(configuration.GetSection("Swarm"));

            // Services.
            //domain
            services.AddScoped<ISwarmService, SwarmService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IVideoService, VideoService>();
            
            //tools
            services.AddScoped<IChunkService, ChunkService>();
            services.AddScoped<IVideoManifestService, VideoManifestService>();

            // Tasks.
            services.AddTransient<IRebuildElasticIndexesTask, RebuildElasticIndexesTask>();
            services.AddTransient<IReindexElasticDocumentsTask, ReindexElasticDocumentsTask>();
            services.AddTransient<IVideoManifestValidatorTask, VideoManifestValidatorTask>();
            
            // Clients.
            services.AddSingleton<ISwarmClient>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var options = sp.GetRequiredService<IOptions<SwarmOptions>>();

                // Use the named client carrying the client-credentials bearer token, so downloads of
                // non-offered (paid) content are authenticated and billed to the Etherna owner account.
                return new SwarmClient(
                    new Uri(options.Value.GatewayUrl),
                    httpClient: httpClientFactory.CreateClient(gatewayHttpClientName));
            });
        }
    }
}
