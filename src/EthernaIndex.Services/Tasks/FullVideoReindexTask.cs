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
