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

using Etherna.DomainEvents;
using Etherna.DomainEvents.Events;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    internal sealed class OnVideoDeletedThenRemoveVideoCommentsHandler(
        IIndexDbContext indexDbContext)
        : EventHandlerBase<EntityDeletedEvent<Video>>
    {
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
