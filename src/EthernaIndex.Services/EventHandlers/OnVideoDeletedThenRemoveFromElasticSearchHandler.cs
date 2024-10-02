// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Elastic.Transport;
using Etherna.DomainEvents;
using Etherna.DomainEvents.Events;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    internal sealed class OnVideoDeletedThenRemoveFromElasticSearchHandler(
        IElasticSearchService elasticSearchService)
        : EventHandlerBase<EntityDeletedEvent<Video>>
    {
        // Methods.
        public override async Task HandleAsync(EntityDeletedEvent<Video> @event)
        {
            try
            {
                await elasticSearchService.DeleteVideoAsync(@event.Entity);
            }
            catch (TransportException)
            { }
        }
    }
}
