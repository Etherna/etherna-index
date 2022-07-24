using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.EthernaIndex.ElasticSearch.DtoModel;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManifestSuccessfulValidatedThenIndexElasticSearchHandler : EventHandlerBase<ManifestSuccessfulValidatedEvent>
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public OnManifestSuccessfulValidatedThenIndexElasticSearchHandler(
            IElasticSearchService elasticSearchService)
        {
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public override async Task HandleAsync(ManifestSuccessfulValidatedEvent @event)
        {
            await elasticSearchService.IndexAsync(
                new VideoManifestElasticDto(
                @event.VideoId,
                @event.Entity.CreationDateTime,
                @event.Entity.Description!,
                @event.Entity.Title!));
        }
    }
}
