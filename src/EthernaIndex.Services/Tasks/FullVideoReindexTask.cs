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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Utility;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    internal sealed class FullVideoReindexTask : IFullVideoReindexTask
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public FullVideoReindexTask(
            IIndexDbContext dbContext,
            IElasticSearchService elasticSearchService)
        {
            this.dbContext = dbContext;
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public async Task RunAsync()
        {
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var videosCursor = await dbContext.Videos.FindAsync<Video>(Builders<Video>.Filter.Empty, new() { NoCursorTimeout = true });
            while (await videosCursor.MoveNextAsync())
                foreach (var element in videosCursor.Current.Where(v => v.LastValidManifest != null))
                    await elasticSearchService.IndexVideoAsync(element);
        }
    }
}
