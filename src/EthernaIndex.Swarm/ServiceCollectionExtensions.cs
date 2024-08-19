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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.EthernaIndex.Swarm
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwarmServices(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            //scoped
            services.Configure<SwarmOptions>(configuration.GetSection("Swarm"));
            services.AddScoped<ISwarmService, SwarmService>();
            
            //singleton
            services.AddSingleton<IBeeClient>(sp =>
            {
                var options = sp.GetRequiredService<SwarmOptions>();
                return new BeeClient(
                    baseUrl: options.GatewayUrl);
            });
        }
    }
}
