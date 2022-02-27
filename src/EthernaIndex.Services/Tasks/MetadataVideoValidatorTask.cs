//   Copyright 2020-present Etherna Sagl
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
using Etherna.EthernaIndex.Domain.Models.ManifestAgg;
using Etherna.EthernaIndex.Swarm;
using Etherna.EthernaIndex.Swarm.DtoModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public class MetadataVideoValidatorTask : IMetadataVideoValidatorTask
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;
        private readonly ISwarmService swarmService;

        // Constructors.
        public MetadataVideoValidatorTask(
            IIndexDbContext indexDbContext,
            ISwarmService swarmService)
        {
            this.indexDbContext = indexDbContext;
            this.swarmService = swarmService;
        }

        // Methods.
        public async Task RunAsync(
            string videoId, 
            string manifestHash,
            bool forceNewValidation)
        {
            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            MetadataVideoDto? metadataDto;
            var validationErrors = new List<ErrorDetail>();

            // Get manifest.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(u => u.Manifest.Hash == manifestHash);
            if (videoManifest.ValidationTime.HasValue &&
                !forceNewValidation)
                return;

            // Get metadata.
            try
            {
                metadataDto = await swarmService.GetMetadataVideoAsync(manifestHash);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                validationErrors.Add(new ErrorDetail(ex switch
                {
                    MetadataVideoException _ => ValidationErrorType.JsonConvert,
                    _ => ValidationErrorType.Generic
                }, ex.Message));
                videoManifest.FailedValidation(validationErrors);
                if (!forceNewValidation)
                    video.AddManifest(videoManifest);
                await indexDbContext.SaveChangesAsync().ConfigureAwait(false);
                return;
            }

            // Check Title.
            if (string.IsNullOrWhiteSpace(metadataDto.Title))
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingTitle, ValidationErrorType.MissingTitle.ToString()));

            // Check Video Format.
            var validationVideoSources = CheckVideoSources(metadataDto.Sources);
            validationErrors.AddRange(validationVideoSources);

            SwarmImageRaw? swarmImageRaw = null;
            if (metadataDto.Thumbnail is not null)
            {
                var validationVideoError = CheckThumbnailSources(metadataDto.Thumbnail.Sources);
                validationErrors.AddRange(validationVideoError);

                swarmImageRaw = new SwarmImageRaw(
                    metadataDto.Thumbnail.AspectRatio,
                    metadataDto.Thumbnail.Blurhash,
                    metadataDto.Thumbnail.Sources);
            }

            // Set result of validation.
            if (validationErrors.Any())
            {
                videoManifest.FailedValidation(validationErrors);
            }
            else
            {
                var videoSources = (metadataDto.Sources ?? Array.Empty<MetadataVideoSourceDto>())
                    .Select(i => new VideoSource(i.Bitrate, i.Quality, i.Reference, i.Size));

                videoManifest.SuccessfulValidation(
                    metadataDto.Description,
                    metadataDto.Duration!.Value,
                    metadataDto.OriginalQuality ?? "",
                    metadataDto.Title,
                    swarmImageRaw,
                    videoSources);
            }

            // Only first validation need to add manifest in video.
            if (!forceNewValidation)
                video.AddManifest(videoManifest);

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

        private IEnumerable<ErrorDetail> CheckVideoSources(IEnumerable<MetadataVideoSourceDto>? videoSources)
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
