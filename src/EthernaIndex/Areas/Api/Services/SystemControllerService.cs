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
    internal sealed class SystemControllerService : ISystemControllerService
    {
        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly ILogger<SystemControllerService> logger;

        // Constructor.
        public SystemControllerService(
            IBackgroundJobClient backgroundJobClient,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IIndexDbContext indexDbContext,
            ILogger<SystemControllerService> logger)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.ethernaOidcClient = ethernaOidcClient;
            this.indexDbContext = indexDbContext;
            this.logger = logger;
        }

        // Methods.
        public async Task ForceVideoManifestValidationAsync(SwarmHash manifestHash)
        {
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(c => c.ManifestHash == manifestHash);
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            backgroundJobClient.Create<IValidateVideoManifestTask>(
                task => task.RunAsync(video.Id, manifestHash.ToString()),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.ForcedVideoManifestsValidation(await ethernaOidcClient.GetClientIdAsync(), video.Id, new[] { manifestHash });
        }

        public async Task ForceVideoManifestsValidationAsync(string videoId)
        {
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == videoId);

            foreach (var manifest in video.VideoManifests)
            {
                backgroundJobClient.Create<IValidateVideoManifestTask>(
                    task => task.RunAsync(video.Id, manifest.ManifestHash.ToString()),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));
            }

            logger.ForcedVideoManifestsValidation(
                await ethernaOidcClient.GetClientIdAsync(),
                video.Id,
                video.VideoManifests.Select(m => m.ManifestHash));
        }
    }
}
