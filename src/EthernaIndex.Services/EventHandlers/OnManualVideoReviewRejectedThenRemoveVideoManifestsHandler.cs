using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManualVideoReviewRejectedThenRemoveVideoManifestsHandler : EventHandlerBase<EntityCreatedEvent<ManualVideoReview>>
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public OnManualVideoReviewRejectedThenRemoveVideoManifestsHandler(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<ManualVideoReview> @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));
            if (@event.Entity.IsValid)
                return;

            var video = await indexDbContext.Videos.FindOneAsync(@event.Entity.Video.Id);

            // Delete unsitable manifests.
            //save manifest list
            var videoManifests = video.VideoManifests.ToList();

            //set video as unsuitable
            video.SetAsUnsuitable();
            await indexDbContext.SaveChangesAsync();

            //remove all VideoManifests from database
            foreach (var videoManifest in videoManifests)
                await indexDbContext.VideoManifests.DeleteAsync(videoManifest);
        }
    }
}
