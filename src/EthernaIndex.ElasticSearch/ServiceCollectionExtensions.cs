using Etherna.EthernaIndex.ElasticSearch.DtoModel;
using Etherna.EthernaIndex.ElasticSearch.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using System.Linq;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public static class ServiceCollectionExtensions
    {
        private const string configurationName = "Elastic";
        public static void AddElasticSearchServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            var elasticSearchsSettingSection = configuration.GetSection(configurationName);
            if (elasticSearchsSettingSection == null)
                return;

            services.Configure<ElasticSearchsSetting>(elasticSearchsSettingSection);

            var elasticSearchsSetting = elasticSearchsSettingSection.Get<ElasticSearchsSetting>();

#pragma warning disable CA2000 // Dispose objects before losing scope
            var settings = new ConnectionSettings(
#pragma warning restore CA2000 // Dispose objects before losing scope
                new Uri(elasticSearchsSetting.Urls.First()))
                .DefaultIndex(elasticSearchsSetting.Indexs.Video);

            AddDefaultMappings(settings);

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
            services.AddScoped<IElasticSearchService, ElasticSearchService>();

            CreateVideoIndex(client, elasticSearchsSetting.Indexs.Video);
        }

        private static void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<VideoManifestElasticDto>(vm => vm
                .IdProperty(p => p.VideoId)
            );
        }

        private static void CreateVideoIndex(IElasticClient client, string indexVideoName)
        {
            var createIndexResponse = client.Indices.Create(indexVideoName,
                index => index.Map<VideoManifestElasticDto>(x => x.AutoMap())
            );
        }
    }
}
