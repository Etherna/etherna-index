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

using Etherna.Authentication;
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
        public async Task ForceVideoManifestValidationAsync(string manifestHash)
        {
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(c => c.Manifest.Hash == manifestHash);
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, manifestHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.ForcedVideoManifestsValidation(await ethernaOidcClient.GetClientIdAsync(), video.Id, new[] { manifestHash });
        }

        public async Task ForceVideoManifestsValidationAsync(string videoId)
        {
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == videoId);

            foreach (var manifest in video.VideoManifests)
            {
                backgroundJobClient.Create<IVideoManifestValidatorTask>(
                    task => task.RunAsync(video.Id, manifest.Manifest.Hash),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));
            }

            logger.ForcedVideoManifestsValidation(await ethernaOidcClient.GetClientIdAsync(), video.Id, video.VideoManifests.Select(m => m.Id));
        }
    }
}
