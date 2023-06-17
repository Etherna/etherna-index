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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Swarm;
using Etherna.EthernaIndex.Swarm.Models;
using Etherna.EthernaIndex.Swarm.Exceptions;
using Etherna.EthernaIndex.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Etherna.EthernaIndex.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

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

            MetadataVideo metadataDto;
            var validationErrors = new List<ErrorDetail>();

            // Get manifest.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(u => u.Manifest.Hash == manifestHash);

            // Get metadata.
            try
            {
#if DEBUG_MOCKUP_SWARM
                swarmService.SetupNewMetadataVideoMockup(manifestHash);
#endif
                metadataDto = await swarmService.GetMetadataVideoAsync(manifestHash);

                logger.VideoManifestValidationRetrievedManifest(videoId, manifestHash);
            }
            catch (MetadataVideoException ex)
            {
                validationErrors.Add(new ErrorDetail(ValidationErrorType.JsonConvert, ex.Message));

                video.FailedManifestValidation(videoManifest, validationErrors);
                await indexDbContext.SaveChangesAsync().ConfigureAwait(false);

                logger.VideoManifestValidationCantRetrieveManifest(videoId, manifestHash, ex);

                return;
            }

            // Validate manifest.
            //description
            if (metadataDto.Description is null)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingDescription));
            else if (metadataDto.Description.Length > VideoManifest.DescriptionMaxLength)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.InvalidDescription, "Description is too long"));

            //duration
            if (metadataDto.Duration == 0)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingDuration));

            //thumbnail
            EthernaIndex.Domain.Models.VideoAgg.SwarmImageRaw? swarmImageRaw = null;
            if (metadataDto.Thumbnail is not null)
            {
                var validationVideoError = CheckThumbnailSources(metadataDto.Thumbnail.Sources, metadataDto.Version);
                validationErrors.AddRange(validationVideoError);

                swarmImageRaw = new EthernaIndex.Domain.Models.VideoAgg.SwarmImageRaw(
                    metadataDto.Thumbnail.AspectRatio,
                    metadataDto.Thumbnail.Blurhash,
                    metadataDto.Thumbnail.Sources.Select(s => new ImageSource(s.Width, s.Type, s.Path)));
            }

            //title
            if (string.IsNullOrWhiteSpace(metadataDto.Title))
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingTitle));
            else if (metadataDto.Title.Length > VideoManifest.TitleMaxLength)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.InvalidTitle, "Title is too long"));

            //aspect ratio
            if (metadataDto.Version.Major > 1 &&
                !metadataDto.AspectRatio.HasValue)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingAspectRatio));

            //manifest creation time
            if (metadataDto.Version.Major > 1 &&
                metadataDto.CreatedAt <= 0)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingManifestCreationTime));

            //video sources
            var videoSourcesErrors = CheckVideoSources(metadataDto.Sources, metadataDto.Version);
            validationErrors.AddRange(videoSourcesErrors);

            //personal data
            if (metadataDto.PersonalData is not null &&
                metadataDto.PersonalData.Length > VideoManifest.PersonalDataMaxLength)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.InvalidPersonalData, "Personal Data is too long"));

            // Set result of validation.
            if (validationErrors.Any())
            {
                video.FailedManifestValidation(videoManifest, validationErrors);

                logger.VideoManifestValidationFailedWithErrors(videoId, manifestHash, null);
            }
            else
            {
                var videoSources = (metadataDto.Sources ?? Array.Empty<MetadataVideoSource>())
                    .Select(i => new VideoSource(i.Quality, i.Path, i.Size, i.Type));

                video.SucceededManifestValidation(
                    videoManifest,
                    metadataDto.AspectRatio,
                    metadataDto.BatchId,
                    metadataDto.Description!,
                    metadataDto.Duration,
                    metadataDto.CreatedAt,
                    metadataDto.UpdatedAt,
                    metadataDto.PersonalData,
                    videoSources,
                    swarmImageRaw,
                    metadataDto.Title);

                logger.VideoManifestValidationSucceeded(videoId, manifestHash);
            }

            // Complete task.
            await indexDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        // Helpers.
        private IEnumerable<ErrorDetail> CheckThumbnailSources(IEnumerable<MetadataImageSource> sources, Version manifestVersion)
        {
            if (sources is null ||
                !sources.Any())
            {
                return new ErrorDetail[] { new ErrorDetail(ValidationErrorType.InvalidThumbnailSource, "Missing sources") };
            }

            var errorDetails = new List<ErrorDetail>();
            foreach (var item in sources)
            {
                if (manifestVersion.Major > 1)
                {
                    if (string.IsNullOrWhiteSpace(item.Path))
                        errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidThumbnailSource, $"[{item.Width}] empty path"));
                    if (string.IsNullOrWhiteSpace(item.Type))
                        errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidThumbnailSource, $"[{item.Width}] empty type"));
                }
                else if (item.Width <= 0)
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidThumbnailSource, $"[{item.Width}] wrong width"));
            }

            return errorDetails;
        }

        private IEnumerable<ErrorDetail> CheckVideoSources(IEnumerable<MetadataVideoSource>? videoSources, Version manifestVersion)
        {
            if (videoSources is null ||
                !videoSources.Any())
            {
                return new ErrorDetail[] { new ErrorDetail(ValidationErrorType.InvalidVideoSource, "Missing sources") };
            }

            var errorDetails = new List<ErrorDetail>();
            foreach (var item in videoSources)
            {
                if (string.IsNullOrWhiteSpace(item.Quality))
                {
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"empty quality"));
                    continue;
                }

                if (manifestVersion.Major > 1)
                {
                    if (string.IsNullOrWhiteSpace(item.Path))
                        errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Quality}] empty path"));
                    if (string.IsNullOrWhiteSpace(item.Type))
                        errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Quality}] empty type"));
                    if (item.Size <= 0)
                        errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Quality}] empty size"));
                }
                else if (string.IsNullOrWhiteSpace(item.Path))
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Quality}] empty reference"));

            }

            return errorDetails;
        }

    }
}
