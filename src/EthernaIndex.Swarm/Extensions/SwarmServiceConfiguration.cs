using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.EthernaIndex.Swarm.Extensions
{
    public static class SwarmServiceConfiguration
    {
        public static void AddSwarmServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));
            
            services.Configure<SwarmSettings>(configuration.GetSection("Swarm"));
            services.AddScoped<ISwarmService, SwarmService>();
        }
    }
}
