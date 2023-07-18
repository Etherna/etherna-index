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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Exceptions;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.EthernaIndex.Swarm;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            //thumbnail
            switch (videoMetadata)
            {
                case VideoManifestMetadataV1 metadataV1:
                    if (metadataV1.Thumbnail?.Sources is not null)
                        validationErrors.AddRange(await CheckThumbnailSourcesAsync(metadataV1.Thumbnail.Sources));
                    break;
                case VideoManifestMetadataV2 metadataV2:
                    if (metadataV2.Thumbnail?.Sources is not null)
                        validationErrors.AddRange(await CheckThumbnailSourcesAsync(metadataV2.Thumbnail.Sources.ToDictionary(i => i.Width.ToString(CultureInfo.InvariantCulture), i => i.Path)));
                    break;
                default: throw new InvalidOperationException();
            }

            // Set result of validation.
            if (validationErrors.Any())
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

        // Helpers.
        private async Task<IEnumerable<ValidationError>> CheckThumbnailSourcesAsync(IReadOnlyDictionary<string, string> sources)
        {
            if (sources is null ||
                !sources.Any())
            {
                return new ValidationError[] { new ValidationError(ValidationErrorType.InvalidVideoSource, "Missing sources") };
            }

            var errorDetails = new List<ValidationError>();
            foreach (var item in sources)
            {
                if (string.IsNullOrWhiteSpace(item.Value))
                    errorDetails.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, $"[{item.Key}] empty source"));

                if (!await swarmService.IsImageAsync(item.Value))
                    errorDetails.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, $"[{item.Key}] source not a valid image format"));
            }

            return errorDetails;
        }
    }
}
