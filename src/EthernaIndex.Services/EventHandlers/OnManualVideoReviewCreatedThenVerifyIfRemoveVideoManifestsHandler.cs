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

using Etherna.DomainEvents;
using Etherna.DomainEvents.Events;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Services.Domain;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    internal sealed class OnManualVideoReviewCreatedThenVerifyIfRemoveVideoManifestsHandler : EventHandlerBase<EntityCreatedEvent<ManualVideoReview>>
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IVideoService videoService;

        // Constructor.
        public OnManualVideoReviewCreatedThenVerifyIfRemoveVideoManifestsHandler(
            IIndexDbContext dbContext,
            IVideoService videoService)
        {
            this.dbContext = dbContext;
            this.videoService = videoService;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<ManualVideoReview> @event)
        {
            ArgumentNullException.ThrowIfNull(@event, nameof(@event));

            if (!@event.Entity.IsValidResult)
            {
                var video = await dbContext.Videos.FindOneAsync(@event.Entity.Video.Id);
                await videoService.ModerateUnsuitableVideoAsync(video);
            }
        }
    }
}
