using Elasticsearch.Net;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.EthernaIndex.ElasticSearch.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using System;

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

            var pool = new StickyConnectionPool(elasticSearchsSetting.Urls.Select(i => new Uri(i)));
            var settings = new ConnectionSettings(pool)
                .DefaultIndex("video")
                .DefaultMappingFor<VideoDocument>(vm => vm.IdProperty(p => p.Id)
            );

            var client = new ElasticClient(settings);

            services.TryAddSingleton<IElasticClient>(client);
            services.TryAddScoped<IElasticSearchService, ElasticSearchService>();

            CreateIndexs(client);
        }

        private static void CreateIndexs(IElasticClient client)
        {
            var createIndexResponse = client.Indices.Create("video",
                index => index.Map<VideoDocument>(x => x.AutoMap())
            );
        }
    }
}
