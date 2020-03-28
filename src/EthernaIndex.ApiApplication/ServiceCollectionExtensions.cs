using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Etherna.EthernaIndex.ApiApplication
{
    public static class ServiceCollectionExtensions
    {
        private const string ServicesSubNamespace = "Services";

        public static void AddApiV1Application(this IServiceCollection services)
        {
            var currentType = typeof(ServiceCollectionExtensions).GetTypeInfo();
            var servicesNamespace = $"{currentType.Namespace}.{ServicesSubNamespace}";

            // Register services.
            foreach (var serviceType in from t in currentType.Assembly.GetTypes()
                                        where t.IsClass && t.Namespace == servicesNamespace && t.DeclaringType == null
                                        select t)
            {
                var serviceInterfaceType = serviceType.GetInterface($"I{serviceType.Name}");
                services.AddScoped(serviceInterfaceType, serviceType);
            }
        }
    }
}
