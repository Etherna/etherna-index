﻿using Elasticsearch.Net;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.EthernaIndex.ElasticSearch.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                .DefaultIndex(elasticSearchsSetting.IndexVideo)
                .DefaultMappingFor<VideoManifestDocument>(vm => vm.IdProperty(p => p.VideoId)
            );

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
            services.AddScoped<IElasticSearchService, ElasticSearchService>();

            CreateVideoIndex(client, elasticSearchsSetting.IndexVideo);
        }

        private static void CreateVideoIndex(IElasticClient client, string indexVideoName)
        {
            var createIndexResponse = client.Indices.Create(indexVideoName,
                index => index.Map<VideoManifestDocument>(x => x.AutoMap())
            );
        }
    }
}
