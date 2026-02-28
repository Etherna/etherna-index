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

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.EthernaIndex.ElasticSearch.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public static class ServiceCollectionExtensions
    {
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "They can't be disposed")]
        public static void AddElasticSearchServices(
            this IServiceCollection services,
            Action<ElasticSearchOptions> elasticSearchOptionsConfig)
        {
            ArgumentNullException.ThrowIfNull(elasticSearchOptionsConfig);
            
            services.Configure(elasticSearchOptionsConfig);

            // Add client.
            services.TryAddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<ElasticSearchOptions>>().Value;
                
                var pool = new StaticNodePool(options.Urls.Select(i => new Uri(i)));
                var settings = new ElasticsearchClientSettings(pool)
                    .DefaultIndex(options.VideosIndexName)
                    .DefaultMappingFor<VideoDocument>(vm => vm.IdProperty(p => p.Id));
                
                return new ElasticsearchClient(settings);
            });

            // Add services.
            services.TryAddTransient<IElasticSearchService, ElasticSearchService>();
        }
    }
}
