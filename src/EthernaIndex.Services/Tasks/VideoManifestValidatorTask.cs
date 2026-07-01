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
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.EthernaIndex.Services.Infrastructure;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Stores;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public class VideoManifestValidatorTask(
        ISwarmClient beeClient,
        IIndexDbContext indexDbContext,
        ILogger<VideoManifestValidatorTask> logger,
        ISwarmService swarmService)
        : IVideoManifestValidatorTask
    {
        // Methods.
        public async Task RunAsync(string videoId, string manifestReference)
        {
            logger.VideoManifestValidationStarted(videoId, manifestReference);

            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            VideoManifestMetadataBase videoMetadata;
            var validationErrors = new List<ValidationError>();

            // Get manifest.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(u => u.ManifestReference == manifestReference);

            // Get video manifest.
            var chunkStore = new SwarmClientChunkStore(beeClient);
#if DEBUG_MOCKUP_SWARM
            swarmService.SetupNewPublishedVideoManifestMockup(manifestReference);
#endif
            var publishedVideoManifest = await swarmService.GetPublishedVideoManifestAsync(manifestReference, chunkStore);

            if (publishedVideoManifest.Manifest is not null)
            {
                //assume is manifest v2, until https://etherna.atlassian.net/browse/EID-240
                videoMetadata = new VideoManifestMetadataV2(
                    publishedVideoManifest.Manifest.Title,
                    publishedVideoManifest.Manifest.Description,
                    (long)publishedVideoManifest.Manifest.Duration.TotalSeconds,
                    publishedVideoManifest.Manifest.VideoSources.Select(vs =>
                        new VideoSourceV2(
                            vs.Uri,
                            vs.Metadata.Quality,
                            vs.Metadata.TotalSourceSize,
                            vs.Metadata.VideoType.ToString())),
                    publishedVideoManifest.Manifest.Thumbnail is null
                        ? null
                        : new ThumbnailV2(
                            publishedVideoManifest.Manifest.Thumbnail.AspectRatio,
                            publishedVideoManifest.Manifest.Thumbnail.Blurhash,
                            publishedVideoManifest.Manifest.Thumbnail.Sources.Select(ts =>
                                new ImageSourceV2(
                                    ts.Metadata.Width,
                                    ts.Uri,
                                    ts.Metadata.ImageType.ToString()))),
                    publishedVideoManifest.Manifest.AspectRatio,
                    publishedVideoManifest.Manifest.CreatedAt.ToUnixTimeSeconds(),
                    publishedVideoManifest.Manifest.UpdatedAt?.ToUnixTimeSeconds(),
                    publishedVideoManifest.Manifest.PersonalDataRaw);

                logger.VideoManifestValidationRetrievedManifest(videoId, manifestReference);
            }
            else
            {
                validationErrors.AddRange(publishedVideoManifest.ValidationErrors
                    .Select(ve => new ValidationError(ve.ErrorType, ve.ErrorMessage)));

                video.FailedManifestValidation(videoManifest, validationErrors);
                await indexDbContext.SaveChangesAsync().ConfigureAwait(false);

                logger.VideoManifestValidationCantRetrieveManifest(videoId, manifestReference, null);

                return;
            }

            // Set result of validation.
            if (validationErrors.Count != 0)
            {
                video.FailedManifestValidation(videoManifest, validationErrors);

                logger.VideoManifestValidationFailedWithErrors(videoId, manifestReference, null);
            }
            else
            {
                video.SucceededManifestValidation(videoManifest, videoMetadata);

                logger.VideoManifestValidationSucceeded(videoId, manifestReference);
            }

            // Complete task.
            await indexDbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
