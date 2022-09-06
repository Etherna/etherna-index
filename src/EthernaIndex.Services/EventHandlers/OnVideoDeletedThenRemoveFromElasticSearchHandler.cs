using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnVideoDeletedThenRemoveFromElasticSearchHandler : EventHandlerBase<EntityDeletedEvent<Video>>
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public OnVideoDeletedThenRemoveFromElasticSearchHandler(
            IElasticSearchService elasticSearchService)
        {
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public override async Task HandleAsync(EntityDeletedEvent<Video> @event)
        {
            await elasticSearchService.RemoveVideoIndexAsync(@event.Entity.Id);
        }
    }
}
