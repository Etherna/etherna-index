using Etherna.DomainEvents;
using Etherna.DomainEvents.Events;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnVideoDeletedThenRemoveVideoCommentsHandler : EventHandlerBase<EntityDeletedEvent<Video>>
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public OnVideoDeletedThenRemoveVideoCommentsHandler(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Methods.
        public override async Task HandleAsync(EntityDeletedEvent<Video> @event)
        {
            var comments = await indexDbContext.Comments.QueryElementsAsync(
                c => c.Where(i => i.Video.Id == @event.Entity.Id)
                          .ToListAsync());

            foreach (var comment in comments)
                await indexDbContext.Comments.DeleteAsync(comment);
        }
    }
}
