using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnVideoModeratedThenRemoveFromElasticSearchHandler : EventHandlerBase<VideoModeratedEvent>
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public OnVideoModeratedThenRemoveFromElasticSearchHandler(
            IElasticSearchService elasticSearchService)
        {
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public override async Task HandleAsync(VideoModeratedEvent @event)
        {
            await elasticSearchService.RemoveVideoIndexAsync(@event.Video);
        }
    }
}
