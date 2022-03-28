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

using Etherna.Authentication.Extensions;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver;
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
    public class ManifestModel : PageModel
    {
        public class VideoManifestDto
        {
            public VideoManifestDto(
                Video video,
                VideoManifest videoManifest)
            {
                if (videoManifest == null)
                    throw new ArgumentNullException(nameof(videoManifest));

                Id = videoManifest.Id;
                CreationDateTime = videoManifest.CreationDateTime;
                Description = videoManifest.Description;
                Duration = videoManifest.Duration;
                ErrorsDetails = videoManifest.ErrorValidationResults.Select(i => $"[{i.ErrorNumber}]: {i.ErrorMessage}");
                IsValid = videoManifest.IsValid;
                ManifestHash = videoManifest.Manifest.Hash;
                OriginalQuality = videoManifest.OriginalQuality;
                OwnerAddress = videoManifest.Id;
                Title = videoManifest.Title ?? videoManifest.Id;
                VideoInfo = new VideoInfoDto(video);
                ValidationTime = videoManifest.ValidationTime;

                Sources = videoManifest.Sources != null ?
                    videoManifest.Sources.Select(i => new MetadataVideoSourceDto(
                        i.Bitrate,
                        i.Reference,
                        i.Size,
                        i.Quality)) : new List<MetadataVideoSourceDto>();
                Thumbnail = videoManifest.Thumbnail != null ? new SwarmImageRawDto
                    (
                        videoManifest.Thumbnail.AspectRatio,
                        videoManifest.Thumbnail.Blurhash,
                        videoManifest.Thumbnail.Sources.ToDictionary(i => i.Key, i => i.Value)
                    ) : null;
            }

            // Properties.
            public string Id { get; set; } = default!;
            public DateTime CreationDateTime { get; set; }
            public string? Description { get; set; } = default!;
            public float? Duration { get; set; }
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
                    float aspectRatio,
                    string blurhash,
                    IReadOnlyDictionary<string, string> sources)
                {
                    AspectRatio = aspectRatio;
                    Blurhash = blurhash;
                    Sources = sources;
                }

                public float AspectRatio { get; set; }
                public string Blurhash { get; set; } = default!;
                public IReadOnlyDictionary<string, string> Sources { get; set; } = default!;
            }

            public class VideoInfoDto
            {
                public VideoInfoDto(Video video)
                {
                    if (video is null)
                        throw new ArgumentNullException(nameof(video));

                    VideoId = video.Id;
                }

                public string VideoId { get; set; }
            }
        }

        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly IUserService userService;

        // Constructor.
        public ManifestModel(
            IBackgroundJobClient backgroundJobClient,
            IIndexDbContext indexDbContext,
            IUserService userService)
        {
            if (indexDbContext is null)
                throw new ArgumentNullException(nameof(indexDbContext));

            this.backgroundJobClient = backgroundJobClient;
            this.indexDbContext = indexDbContext;
            this.userService = userService;
        }

        // Properties.
        public VideoManifestDto VideoManifest { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string manifestHash)
        {
            await InitializeAsync(manifestHash);
        }

        public async Task<IActionResult> OnPostApproveVideo(
            string videoId,
            string manifestHash)
        {
            await CreateReviewAsync(videoId, manifestHash, true);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRejectVideo(
            string videoId,
            string manifestHash)
        {
            await CreateReviewAsync(videoId, manifestHash, false);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostForceNewValidationAsync(string manifestHash)
        {
            // Get Manifest & Video data.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(c => c.Manifest.Hash == manifestHash);
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            // Background Validator.
            backgroundJobClient.Create<IMetadataVideoValidatorTask>(
                    task => task.RunAsync(video.Id, videoManifest.Manifest.Hash),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync(string manifestHash)
        {
            // Video info
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.Manifest.Hash == manifestHash);
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            VideoManifest = new VideoManifestDto(video, videoManifest);
        }

        private async Task CreateReviewAsync(string videoId, string manifestHash, bool isValid)
        {
            var address = HttpContext!.User.GetEtherAddress();
            var (user, _) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(videoId);
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.Manifest.Hash == manifestHash);

            // Create ManualReview.
            await indexDbContext.ManualVideoReviews.CreateAsync(new ManualVideoReview(user, "", isValid, video, videoManifest));
        }
    }
}
