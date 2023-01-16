﻿//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    internal sealed class OnCommentCreatedThenIndexToElasticSearchHandler : EventHandlerBase<EntityCreatedEvent<Comment>>
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
