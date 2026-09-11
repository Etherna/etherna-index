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

using Elastic.Transport;
using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    internal sealed class OnCommentTextUpdatedThenIndexVideoToElasticSearchHandler(
        IElasticSearchService elasticSearchService,
        IIndexDbContext indexDbContext)
        : EventHandlerBase<CommentTextUpdatedEvent>
    {
        // Methods.
        public override async Task HandleAsync(CommentTextUpdatedEvent @event)
        {
            // Reload the video, because the comment could keep only a partial reference of it.
            // The video could also have been deleted concurrently: the event is dispatched after
            // the comment has been persisted, so tolerate a missing video instead of failing the
            // user request.
            var video = await indexDbContext.Videos.TryFindOneAsync(@event.Comment.Video.Id);

            // A video without a valid manifest is not indexed, so there is no document to refresh.
            if (video?.LastValidManifest is null)
                return;

            try
            {
                await elasticSearchService.AddVideoAsync(video);
            }
            catch (TransportException)
            { }
        }
    }
}
