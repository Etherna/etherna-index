using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnCommentCreatedThenIndexToElasticSearchHandler : EventHandlerBase<EntityCreatedEvent<Comment>>
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public OnCommentCreatedThenIndexToElasticSearchHandler(
            IElasticSearchService elasticSearchService)
        {
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<Comment> @event)
        {
            await elasticSearchService.IndexCommentAsync(@event.Entity);
        }
    }
}
