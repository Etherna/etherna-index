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

using Etherna.Authentication;
using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.EthernaIndex.Services.Tasks;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal sealed class SystemControllerService(
        IBackgroundJobClient backgroundJobClient,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IIndexDbContext indexDbContext,
        ILogger<SystemControllerService> logger)
        : ISystemControllerService
    {
        // Methods.
        public async Task ForceVideoManifestValidationAsync(SwarmReference manifestReference)
        {
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(c => c.ManifestReference == manifestReference);
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, manifestReference.ToString()),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.ForcedVideoManifestsValidation(await ethernaOidcClient.GetClientIdAsync(), video.Id, [manifestReference]);
        }

        public async Task ForceVideoManifestsValidationAsync(string videoId)
        {
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == videoId);

            foreach (var manifest in video.VideoManifests)
            {
                backgroundJobClient.Create<IVideoManifestValidatorTask>(
                    task => task.RunAsync(video.Id, manifest.ManifestReference.ToString()),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));
            }

            logger.ForcedVideoManifestsValidation(
                await ethernaOidcClient.GetClientIdAsync(),
                video.Id,
                video.VideoManifests.Select(m => m.ManifestReference));
        }
    }
}
