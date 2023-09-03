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
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            
            if (!@event.Entity.IsValidResult)
            {
                var video = await dbContext.Videos.FindOneAsync(@event.Entity.Video.Id);
                await videoService.ModerateUnsuitableVideoAsync(video);
            }
        }
    }
}
