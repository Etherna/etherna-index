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
    internal sealed class RebuildElasticIndexesTask(
        IIndexDbContext dbContext,
        IElasticSearchService elasticSearchService)
        : IRebuildElasticIndexesTask
    {
        // Methods.
        public async Task RunAsync()
        {
            // Destroy and recreate indexes.
            await elasticSearchService.DestroyIndexesAsync();
            await elasticSearchService.CreateIndexesAsync();
            
            // Reindex documents.
            //comments
            var commentsCursor = await dbContext.Comments.FindAsync<Comment>(Builders<Comment>.Filter.Empty, new() { NoCursorTimeout = true });
            while (await commentsCursor.MoveNextAsync())
                foreach (var comment in commentsCursor.Current)
                    await elasticSearchService.AddCommentAsync(comment);
            
            //videos
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var videosCursor = await dbContext.Videos.FindAsync<Video>(Builders<Video>.Filter.Empty, new() { NoCursorTimeout = true });
            while (await videosCursor.MoveNextAsync())
                foreach (var video in videosCursor.Current.Where(v => v.LastValidManifest != null))
                    await elasticSearchService.AddVideoAsync(video);
        }
    }
}
