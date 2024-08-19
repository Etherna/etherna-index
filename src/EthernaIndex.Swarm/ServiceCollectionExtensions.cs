//   Copyright 2021-present Etherna Sagl
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
