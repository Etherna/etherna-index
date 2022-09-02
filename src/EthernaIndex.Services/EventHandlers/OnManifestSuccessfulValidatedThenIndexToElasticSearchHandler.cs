using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManifestSuccessfulValidatedThenIndexToElasticSearchHandler : EventHandlerBase<ManifestSuccessfulValidatedEvent>
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public OnManifestSuccessfulValidatedThenIndexToElasticSearchHandler(
            IElasticSearchService elasticSearchService)
        {
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public override async Task HandleAsync(ManifestSuccessfulValidatedEvent @event)
        {
            await elasticSearchService.IndexVideoAsync(@event.Video);
        }
    }
}
