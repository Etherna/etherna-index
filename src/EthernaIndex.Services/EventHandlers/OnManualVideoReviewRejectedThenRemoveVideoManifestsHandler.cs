using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManualVideoReviewRejectedThenRemoveVideoManifestsHandler : EventHandlerBase<ManualVideoReviewRejectedEvent>
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
        public override async Task HandleAsync(ManualVideoReviewRejectedEvent @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));

            // Set video status to InvalidContent.
            @event.Video.SetAsUnsuitable();

            // Remove all VideoManifests from video and database.
            var manifestIds = @event.Video.VideoManifests.Select(vm => vm.Id);
            var videoManifests = await indexDbContext.VideoManifests.QueryElementsAsync(elements =>
                elements.Where(vm => manifestIds.Contains(vm.Id))
                .ToListAsync());

            foreach (var videoManifest in videoManifests)
                await indexDbContext.VideoManifests.DeleteAsync(videoManifest);

            await indexDbContext.SaveChangesAsync();
        }
    }
}
