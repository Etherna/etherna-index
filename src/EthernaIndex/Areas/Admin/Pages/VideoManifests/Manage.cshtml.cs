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
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver.Linq;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class ManageModel : PageModel
    {
        public class VideoManifestDto
        {
            public VideoManifestDto(
                string id,
                bool? contentApproved,
                DateTime creationDateTime,
                string? description,
                int? duration,
                IEnumerable<string> errorsDetails,
                bool? isValid,
                string manifestHash,
                string? originalQuality,
                string ownerAddress,
                IEnumerable<MetadataVideoSourceDto>? sources,
                SwarmImageRawDto? thumbnail,
                string? title,
                VideoInfoDto videoInfo,
                DateTime? validationTime)
            {
                Id = id;
                ContentApproved = contentApproved;
                CreationDateTime = creationDateTime;
                Description = description;
                Duration = duration;
                ErrorsDetails = errorsDetails;
                IsValid = isValid;
                ManifestHash = manifestHash;
                OriginalQuality = originalQuality;
                OwnerAddress = ownerAddress;
                Sources = sources ?? new List<MetadataVideoSourceDto>();
                Thumbnail = thumbnail;
                Title = title;
                VideoInfo = videoInfo;
                ValidationTime = validationTime;
            }

            // Properties.
            public string Id { get; set; } = default!;
            public bool? ContentApproved { get; set; }
            public DateTime CreationDateTime { get; set; }
            public string? Description { get; set; } = default!;
            public int? Duration { get; set; }
            public IEnumerable<string> ErrorsDetails { get; set; } = default!;
            public bool? IsValid { get; set; }
            public string ManifestHash { get; set; } = default!;
            public string? OriginalQuality { get; set; }
            public string OwnerAddress { get; set; } = default!;
            public IEnumerable<MetadataVideoSourceDto> Sources { get; set; }
            public SwarmImageRawDto? Thumbnail { get; set; }
            public string? Title { get; set; } = default!;
            public VideoInfoDto VideoInfo { get; set; }
            public DateTime? ValidationTime { get; set; }

            // Methods.
            static public VideoManifestDto FromManifestEntity(VideoManifest videoManifest, VideoInfoDto videoInfoDto)
            {
                if (videoManifest == null)
                    throw new ArgumentNullException(nameof(videoManifest));

                return new VideoManifestDto(
                    videoManifest.Id,
                    videoManifest.ContentApproved,
                    videoManifest.CreationDateTime,
                    videoManifest.Description,
                    videoManifest.Duration,
                    videoManifest.ErrorValidationResults.Select(i => $"[{i.ErrorNumber}]: {i.ErrorMessage}"),
                    videoManifest.IsValid,
                    videoManifest.ManifestHash.Hash,
                    videoManifest.OriginalQuality,
                    "",
                    videoManifest.Sources?.Select(i => new MetadataVideoSourceDto(
                        i.Bitrate,
                        i.Reference,
                        i.Size,
                        i.Quality))?.ToList(),
                    videoManifest.Thumbnail != null ? new SwarmImageRawDto
                    (
                        videoManifest.Thumbnail.AspectRatio, 
                        videoManifest.Thumbnail.BlurHash,
                        videoManifest.Thumbnail.Sources.ToDictionary(i => i.Key, i => i.Value)
                    ) : null,
                    videoManifest.Title,
                    videoInfoDto,
                    videoManifest.ValidationTime);
            }

            public class MetadataVideoSourceDto
            {
                public MetadataVideoSourceDto(
                    int? bitrate,
                    string reference,
                    int? size,
                    string quality)
                {
                    Bitrate = bitrate;
                    Reference = reference;
                    Size = size;
                    Quality = quality;
                }

                public int? Bitrate { get; set; }
                public string Reference { get; set; } = default!;
                public int? Size { get; set; }
                public string Quality { get; set; } = default!;
            }

            public class SwarmImageRawDto
            {
                public SwarmImageRawDto(
                    int aspectRatio,
                    string blurhash,
                    IReadOnlyDictionary<string, string> sources)
                {
                    AspectRatio = aspectRatio;
                    Blurhash = blurhash;
                    Sources = sources;
                }

                public int AspectRatio { get; set; }
                public string Blurhash { get; set; } = default!;
                public IReadOnlyDictionary<string, string> Sources { get; set; } = default!;
            }

            public class VideoInfoDto
            {
                public VideoInfoDto(
                    string videoId,
                    ContentReviewType? contentReview)
                {
                    VideoId = videoId;
                    ContentReview = contentReview;
                }

                public string VideoId { get; set; }
                public ContentReviewType? ContentReview { get; set; }
            }
        }

        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IIndexDbContext dbContext;

        // Constructor.
        public ManageModel(
            IBackgroundJobClient backgroundJobClient,
            IIndexDbContext dbContext)
        {
            if (dbContext is null)
                throw new ArgumentNullException(nameof(dbContext));

            this.backgroundJobClient = backgroundJobClient;
            this.dbContext = dbContext;
        }

        // Properties.
        public VideoManifestDto VideoManifest { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string manifestHash)
        {
            await InitializeAsync(manifestHash);
        }

        public async Task<IActionResult> OnPostManageVideoReportAsync(
            string manifestHash,
            string button)
        {
            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(button))
                return RedirectToPage("Index");

            if (button.Equals("Force New Validation", StringComparison.Ordinal))
                await ForceNewValidationAsync(manifestHash);
            else if (button.Equals("Force Invalid Status", StringComparison.Ordinal))
                await ForceStatusAsync(manifestHash, false);
            else if (button.Equals("Force Valid Status", StringComparison.Ordinal))
                await ForceStatusAsync(manifestHash, true);

            await dbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task ForceNewValidationAsync(string manifestHash)
        {
            var videoManifest = await dbContext.VideoManifests.TryFindOneAsync(c => c.ManifestHash.Hash == manifestHash);
            if (videoManifest is not null)
                backgroundJobClient.Create<MetadataVideoValidatorTask>(
                    task => task.RunAsync(videoManifest.Video.Id, videoManifest.ManifestHash.Hash),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));
        }

        private async Task ForceStatusAsync(string manifestHash, bool isValid)
        {
            var videoManifest = await dbContext.VideoManifests.TryFindOneAsync(c => c.ManifestHash.Hash == manifestHash);
            if (videoManifest is null)
                return;

            videoManifest.ForceStatus(isValid);
        }

        private async Task InitializeAsync(string manifestHash)
        {
            // Video info
            var videoManifest = await dbContext.VideoManifests.FindOneAsync(i => i.ManifestHash.Hash == manifestHash);
            var video = await dbContext.Videos.FindOneAsync(i => i.Id == videoManifest.Video.Id);

            VideoManifest = VideoManifestDto.FromManifestEntity(
                videoManifest, 
                new VideoManifestDto.VideoInfoDto(
                    video.Id,
                    video.ContentReview));
        }

    }
}
