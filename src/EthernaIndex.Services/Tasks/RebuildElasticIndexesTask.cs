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
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    internal sealed class RebuildElasticIndexesTask(
        IIndexDbContext dbContext,
        IElasticSearchService elasticSearchService)
        : IRebuildElasticIndexesTask
    {
        // Methods.
        public async Task RunAsync()
        {
            // Drop and recreate the indexes from scratch. This applies any mapping/settings change and
            // wipes stale documents, at the cost of leaving the indexes unavailable until the reindex ends.
            await elasticSearchService.DestroyIndexesAsync();
            await elasticSearchService.CreateIndexesAsync();

            // Reindex documents into the fresh indexes.
            /* The scan reads the whole collection: a transient models scope per cursor batch evicts the
             * videos, and the manifests they preload, once indexed, so the identity map doesn't grow
             * with the collection while the models stay tracked for the explicit preloads. */
            var videosCursor = await dbContext.Videos.FindAsync<Video>(Builders<Video>.Filter.Empty, new() { NoCursorTimeout = true });
            while (true)
            {
                using var transientModelsScope = dbContext.StartTransientModelsScope();
                if (!await videosCursor.MoveNextAsync())
                    break;

                foreach (var video in videosCursor.Current.Where(v => v.LastValidManifest != null))
                    await elasticSearchService.AddVideoAsync(video);
            }
        }
    }
}
