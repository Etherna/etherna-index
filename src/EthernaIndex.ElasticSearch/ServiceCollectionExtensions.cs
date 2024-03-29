﻿//   Copyright 2021-present Etherna Sagl
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

using Elasticsearch.Net;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.EthernaIndex.ElasticSearch.Configs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public static class ServiceCollectionExtensions
    {
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "They can't be disposed")]
        public static void AddElasticSearchServices(
            this IServiceCollection services,
            IEnumerable<string> urls,
            Action<ElasticSearchOptions> elasticSearchOptionsConfig)
        {
            var options = new ElasticSearchOptions(urls);
            elasticSearchOptionsConfig?.Invoke(options);

            var pool = new StickyConnectionPool(options.Urls.Select(i => new Uri(i)));
            var settings = new ConnectionSettings(pool)
                .DefaultIndex(options.VideosIndexName)
                .DefaultMappingFor<VideoDocument>(vm => vm.IdProperty(p => p.Id)
            );
            var client = new ElasticClient(settings);

            // Add services.
            services.TryAddSingleton<IElasticClient>(client);
            services.TryAddScoped<IElasticSearchService, ElasticSearchService>();

            // Create indexes.
            client.Indices.Create(options.CommentsIndexName,
                index => index.Map<CommentDocument>(x => x.AutoMap())
            );
            client.Indices.Create(options.VideosIndexName,
                index => index.Map<VideoDocument>(x => x.AutoMap())
            );
        }
    }
}
