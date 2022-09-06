using Elasticsearch.Net;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.EthernaIndex.ElasticSearch.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

#pragma warning disable CA2000 // Can't dispose registration service 
            var pool = new StickyConnectionPool(elasticSearchsSetting.Urls.Select(i => new Uri(i)));
            var settings = new ConnectionSettings(pool)
                .DefaultIndex("video")
                .DefaultMappingFor<VideoDocument>(vm => vm.IdProperty(p => p.Id)
            );
#pragma warning restore CA2000
            var client = new ElasticClient(settings);

            // Add services.
            services.TryAddSingleton<IElasticClient>(client);
            services.TryAddScoped<IElasticSearchService, ElasticSearchService>();

            // Create indexes.
            client.Indices.Create("comment",
                index => index.Map<CommentDocument>(x => x.AutoMap())
            );
            client.Indices.Create("video",
                index => index.Map<VideoDocument>(x => x.AutoMap())
            );
        }
    }
}
