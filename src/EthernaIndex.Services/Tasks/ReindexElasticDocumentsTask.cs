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
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    internal sealed class ReindexElasticDocumentsTask(
        IIndexDbContext dbContext,
        IElasticSearchService elasticSearchService)
        : IReindexElasticDocumentsTask
    {
        // Methods.
        public async Task RunAsync()
        {
            // A task has no user request scope, so open a db execution context manually before any
            // db operation, wrapping the whole task body.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext);

            // Make sure indexes exist with their expected mappings, without dropping existing
            // data: this keeps the reindex a zero-downtime, in-place operation.
            await elasticSearchService.CreateIndexesAsync();

            // Take the instant the reindex begins. Every document (re)written from here on gets
            // its IndexingDateTime stamped to "now", so anything still carrying an older stamp
            // afterwards was not refreshed and is therefore an orphan to be pruned.
            var reindexStartedAt = DateTime.UtcNow;

            // Reindex documents in place (existing documents are overwritten, not removed first).
            var videosCursor = await dbContext.Videos.FindAsync<Video>(Builders<Video>.Filter.Empty, new() { NoCursorTimeout = true });
            while (await videosCursor.MoveNextAsync())
                foreach (var video in videosCursor.Current.Where(v => v.LastValidManifest != null))
                    await elasticSearchService.AddVideoAsync(video);

            // Prune orphan documents: anything still indexed with a stamp older than the start of
            // this run no longer exists in the primary store (or lost its last valid manifest).
            // The enumeration and removal run entirely server-side on Elasticsearch.
            await elasticSearchService.RemoveVideoDocumentsIndexedBeforeAsync(reindexStartedAt);
        }
    }
}
