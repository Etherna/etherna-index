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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Exceptions;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.EthernaIndex.Swarm;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public class VideoManifestValidatorTask : IVideoManifestValidatorTask
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;
        private readonly ILogger<VideoManifestValidatorTask> logger;
        private readonly ISwarmService swarmService;

        // Constructors.
        public VideoManifestValidatorTask(
            IIndexDbContext indexDbContext,
            ILogger<VideoManifestValidatorTask> logger,
            ISwarmService swarmService)
        {
            this.indexDbContext = indexDbContext;
            this.logger = logger;
            this.swarmService = swarmService;
        }

        // Methods.
        public async Task RunAsync(string videoId, string manifestHash)
        {
            logger.VideoManifestValidationStarted(videoId, manifestHash);

            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            VideoManifestMetadataBase videoMetadata;
            var validationErrors = new List<ValidationError>();

            // Get manifest.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(u => u.Manifest.Hash == manifestHash);

            // Get metadata.
            try
            {
#if DEBUG_MOCKUP_SWARM
                swarmService.SetupNewMetadataV2VideoMockup(manifestHash);
#endif
                videoMetadata = await swarmService.GetVideoMetadataAsync(manifestHash);

                logger.VideoManifestValidationRetrievedManifest(videoId, manifestHash);
            }
            catch (VideoManifestValidationException ex)
            {
                validationErrors.AddRange(ex.ValidationErrors);

                video.FailedManifestValidation(videoManifest, validationErrors);
                await indexDbContext.SaveChangesAsync().ConfigureAwait(false);

                logger.VideoManifestValidationCantRetrieveManifest(videoId, manifestHash, ex);

                return;
            }

            // Set result of validation.
            if (validationErrors.Count != 0)
            {
                video.FailedManifestValidation(videoManifest, validationErrors);

                logger.VideoManifestValidationFailedWithErrors(videoId, manifestHash, null);
            }
            else
            {
                video.SucceededManifestValidation(videoManifest, videoMetadata);

                logger.VideoManifestValidationSucceeded(videoId, manifestHash);
            }

            // Complete task.
            await indexDbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
