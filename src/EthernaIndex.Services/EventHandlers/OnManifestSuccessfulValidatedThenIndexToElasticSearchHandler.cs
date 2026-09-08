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
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.ElasticSearch;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.EventHandlers
{
    internal sealed class OnManifestSuccessfulValidatedThenIndexToElasticSearchHandler(
        IElasticSearchService elasticSearchService,
        IIndexDbContext indexDbContext)
        : EventHandlerBase<ManifestSuccessfulValidatedEvent>
    {
        // Methods.
        public override async Task HandleAsync(ManifestSuccessfulValidatedEvent @event)
        {
            // Reload the video in this scope: the event carries the instance of the saving scope, whose
            // references are summaries again after the save. The video could also have been deleted
            // meanwhile, or have lost its last valid manifest.
            var video = await indexDbContext.Videos.TryFindOneAsync(@event.Video.Id);
            if (video?.LastValidManifest is null)
                return;

            try
            {
                await elasticSearchService.AddVideoAsync(video);
            }
            catch (TransportException)
            { }
        }
    }
}
