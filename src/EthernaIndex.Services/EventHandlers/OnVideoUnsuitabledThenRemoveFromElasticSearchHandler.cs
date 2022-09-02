using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnVideoUnsuitabledThenRemoveFromElasticSearchHandler : EventHandlerBase<VideoUnsuitabledEvent>
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public OnVideoUnsuitabledThenRemoveFromElasticSearchHandler(
            IElasticSearchService elasticSearchService)
        {
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public override async Task HandleAsync(VideoUnsuitabledEvent @event)
        {
            await elasticSearchService.RemoveVideoIndexAsync(@event.Video.Id);
        }
    }
}
