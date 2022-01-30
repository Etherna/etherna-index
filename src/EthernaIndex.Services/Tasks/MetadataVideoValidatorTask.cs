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
using Etherna.EthernaIndex.Domain.Models.Manifest;
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
        private readonly IIndexContext indexContext;
        private readonly ISwarmService swarmService;

        // Constructors.
        public MetadataVideoValidatorTask(
            IIndexContext indexContext,
            ISwarmService swarmService)
        {
            this.indexContext = indexContext;
            this.swarmService = swarmService;
        }

        // Methods.
        public async Task RunAsync(string manifestHash)
        {
            var video = await indexContext.Videos.FindOneAsync(u => u.ManifestHash.Hash == manifestHash);

            MetadataVideoDto? metadataDto;
            var validationErrors = new List<ErrorDetail>();

            //get manifest
            var videoManifest = video.VideoManifest.FirstOrDefault(i => i.ManifestHash == manifestHash);
            if (videoManifest is null)
            {
                var ex = new InvalidOperationException("Manifest not found");
                ex.Data.Add("ManifestHash", manifestHash);
                throw ex;
            }
            if (videoManifest.IsValid.HasValue)
            {
                var ex = new InvalidOperationException("Manifest already processed");
                ex.Data.Add("ManifestHash", manifestHash);
                throw ex;
            }

            //get manifest metadata
            try
            {
                metadataDto = await swarmService.GetMetadataVideoAsync(manifestHash);
            }
            catch (MetadataVideoException ex)
            {
                validationErrors.Add(new ErrorDetail(ValidationErrorType.JsonConvert, ex.Message));
                videoManifest.FailedValidation(validationErrors);
                await indexContext.SaveChangesAsync().ConfigureAwait(false);
                return;
            }

            //check Title
            if (string.IsNullOrWhiteSpace(metadataDto.Title))
                validationErrors.Add(new ErrorDetail(ValidationErrorType.MissingTitle, ValidationErrorType.MissingTitle.ToString()));

            //check Video Format
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

            //set result of validation
            if (validationErrors.Any())
            {
                videoManifest.FailedValidation(validationErrors);
            }
            else
            {
                //inizializeManifest
                var videoSources = metadataDto.Sources?.Select(i => new VideoSource(i.Bitrate, i.Quality, i.Reference, i.Size)).ToList();

                videoManifest.SuccessfulValidation(
                    metadataDto.Id,
                    metadataDto.Title,
                    metadataDto.Description,
                    metadataDto.OriginalQuality,
                    metadataDto.Duration,
                    swarmImageRaw);
            }

            // Complete task.
            await indexContext.SaveChangesAsync().ConfigureAwait(false);
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

            return Array.Empty<ErrorDetail>();
        }

        private IEnumerable<ErrorDetail> CheckVideoSources(IEnumerable<MetadataVideoSourceDto> videoSources)
        {
            if (videoSources is null ||
                !videoSources.Any())
            {
                return new ErrorDetail[] { new ErrorDetail(ValidationErrorType.InvalidVideoSource, "Missing sources") };
            }

            var errorDetails = new List<ErrorDetail>();
            foreach (var item in videoSources)
            {
                if (string.IsNullOrWhiteSpace(item.Reference))
                    errorDetails.Add(new ErrorDetail(ValidationErrorType.InvalidVideoSource, $"[{item.Quality}] empty reference"));
            }

            return Array.Empty<ErrorDetail>();
        }

    }
}
