using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Services.Domain;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManualVideoReviewRejectedThenRemoveVideoManifestsHandler : EventHandlerBase<EntityCreatedEvent<ManualVideoReview>>
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IVideoService videoService;

        // Constructor.
        public OnManualVideoReviewRejectedThenRemoveVideoManifestsHandler(
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
