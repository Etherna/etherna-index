using Etherna.DomainEvents;
using Etherna.DomainEvents.Events;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManualVideoReviewCreatedThenArchiveRelatedVideoReportsHandler : EventHandlerBase<EntityCreatedEvent<ManualVideoReview>>
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public OnManualVideoReviewCreatedThenArchiveRelatedVideoReportsHandler(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<ManualVideoReview> @event)
        {
            var unsuitableVideoReports = await indexDbContext.UnsuitableVideoReports.QueryElementsAsync(
                uvr => uvr.Where(i => i.Video.Id == @event.Entity.VideoId)
                          .ToListAsync());

            foreach (var item in unsuitableVideoReports)
                item.SetArchived();

            await indexDbContext.SaveChangesAsync();
        }
    }
}
