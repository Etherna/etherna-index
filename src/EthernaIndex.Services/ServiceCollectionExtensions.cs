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

using Etherna.BeeNet;
using Etherna.BeeNet.Services;
using Etherna.DomainEvents;
using Etherna.DomainEvents.AspNetCore;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Infrastructure;
using Etherna.EthernaIndex.Services.Options;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.Sdk.Tools.Video.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;

namespace Etherna.EthernaIndex.Services
{
    public static class ServiceCollectionExtensions
    {
        private const string EventHandlersSubNamespace = "EventHandlers";

        public static void AddDomainServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            
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
            services.AddTransient<IVideoManifestValidatorTask, VideoManifestValidatorTask>();
            
            // Clients.
            services.AddSingleton<IBeeClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<SwarmOptions>>();
                return new BeeClient(new Uri(options.Value.GatewayUrl));
            });
        }
    }
}
