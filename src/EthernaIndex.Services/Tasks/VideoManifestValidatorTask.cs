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
                (metadataDto, bool isFeed) = await swarmService.GetMetadataVideoAsync(manifestHash);

                logger.VideoManifestValidationRetrievedManifest(videoId, manifestHash);

                if (isFeed)
                    validationErrors.Add(new ErrorDetail(ValidationErrorType.IsFeedContent, "Hash content is on a feed"));
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

            //original quality
            if (string.IsNullOrWhiteSpace(metadataDto.OriginalQuality))
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingOriginalQuality));

            //thumbnail
            EthernaIndex.Domain.Models.VideoAgg.SwarmImageRaw? swarmImageRaw = null;
            if (metadataDto.Thumbnail is not null)
            {
                var validationVideoError = CheckThumbnailSources(metadataDto.Thumbnail.Sources);
                validationErrors.AddRange(validationVideoError);

                swarmImageRaw = new EthernaIndex.Domain.Models.VideoAgg.SwarmImageRaw(
                    metadataDto.Thumbnail.AspectRatio,
                    metadataDto.Thumbnail.Blurhash,
                    metadataDto.Thumbnail.Sources);
            }

            //title
            if (string.IsNullOrWhiteSpace(metadataDto.Title))
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingTitle));
            else if (metadataDto.Title.Length > VideoManifest.TitleMaxLength)
                validationErrors.Add(new ErrorDetail(ValidationErrorType.InvalidTitle, "Title is too long"));

            //video sources
            var videoSourcesErrors = CheckVideoSources(metadataDto.Sources);
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
                    .Select(i => new VideoSource(i.Bitrate, i.Quality, i.Reference, i.Size));

                video.SucceededManifestValidation(
                    videoManifest,
                    metadataDto.BatchId,
                    metadataDto.Description!,
                    metadataDto.Duration,
                    metadataDto.OriginalQuality,
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
        private IEnumerable<ErrorDetail> CheckThumbnailSources(IReadOnlyDictionary<string, string> sources)
        {
            if (sources is null ||
                !sources.Any())
            {
                return new ErrorDetail[] { new ErrorDetail(ValidationErrorType.InvalidVideoSource, "Missing sources") };
            }

            var errorDetails = new List<ErrorDetail>();
            foreach (var item in sources)
            {
                if (string.IsNullOrWhiteSpace(item.Value))
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Key}] empty source"));
            }

            return errorDetails;
        }

        private IEnumerable<ErrorDetail> CheckVideoSources(IEnumerable<MetadataVideoSource>? videoSources)
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
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"empty quality"));
                else if (string.IsNullOrWhiteSpace(item.Reference))
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Quality}] empty reference"));
            }

            return errorDetails;
        }

    }
}
