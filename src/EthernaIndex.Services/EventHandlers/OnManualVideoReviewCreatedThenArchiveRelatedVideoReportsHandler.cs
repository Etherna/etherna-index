//   Copyright 2021-present Etherna Sagl
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
using Etherna.DomainEvents.Events;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    class OnManualVideoReviewCreatedThenArchiveRelatedVideoReportsHandler : EventHandlerBase<EntityCreatedEvent<ManualVideoReview>>
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public OnManualVideoReviewCreatedThenArchiveRelatedVideoReportsHandler(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Methods.
        public override async Task HandleAsync(EntityCreatedEvent<ManualVideoReview> @event)
        {
            var unsuitableVideoReports = await indexDbContext.UnsuitableVideoReports.QueryElementsAsync(
                uvr => uvr.Where(i => i.Video.Id == @event.Entity.Video.Id)
                          .ToListAsync());

            foreach (var item in unsuitableVideoReports)
                item.SetArchived();

            await indexDbContext.SaveChangesAsync();
        }
    }
}
