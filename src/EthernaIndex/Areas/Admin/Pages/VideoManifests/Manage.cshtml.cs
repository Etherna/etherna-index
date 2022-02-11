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
            // Properties.
            public bool? ContentApproved { get; set; }
            public DateTime CreationDateTime { get; set; }
            public string? Description { get; set; } = default!;
            public int? Duration { get; set; }
            public IEnumerable<string> ErrorsDetails { get; set; } = default!;
            public string Id { get; set; } = default!;
            public bool? IsValid { get; set; }
            public string ManifestHash { get; set; } = default!;
            public string? OriginalQuality { get; set; }
            public string OwnerAddress { get; set; } = default!;
            public IEnumerable<MetadataVideoSourceDto> Sources { get; set; } = default!;
            public SwarmImageRawDto Thumbnail { get; set; } = default!;
            public string? Title { get; set; } = default!;
            public DateTime? ValidationTime { get; set; }
            public string VideoId { get; set; } = default!;

            // Methods.
            static public VideoManifestDto FromManifestEntity(VideoManifest videoManifest)
            {
                if (videoManifest == null)
                    return new VideoManifestDto();

                return new VideoManifestDto
                {
                    ContentApproved = videoManifest.ContentApproved,
                    CreationDateTime = videoManifest.CreationDateTime,
                    Description = videoManifest.Description,
                    Duration = videoManifest.Duration,
                    Id = videoManifest.Id,
                    IsValid = videoManifest.IsValid,
                    ErrorsDetails = videoManifest.ErrorValidationResults.Select(i => $"[{i.ErrorNumber}]: {i.ErrorMessage}"),
                    ManifestHash = videoManifest.ManifestHash.Hash,
                    OriginalQuality = videoManifest.OriginalQuality,
                    OwnerAddress = "",
                    Sources = videoManifest.Sources?.Select(i => new MetadataVideoSourceDto
                    {
                        Bitrate = i.Bitrate,
                        Quality = i.Quality,
                        Reference = i.Reference,
                        Size = i.Size
                    })?.ToList() ?? new List<MetadataVideoSourceDto>(),
                    Thumbnail = videoManifest.Thumbnail != null ? new SwarmImageRawDto
                    {
                        AspectRatio = videoManifest.Thumbnail.AspectRatio,
                        Blurhash = videoManifest.Thumbnail.BlurHash,
                        Sources = videoManifest.Thumbnail.Sources.ToDictionary(i => i.Key, i => i.Value)
                    } : new SwarmImageRawDto(),
                    Title = videoManifest.Title,
                    ValidationTime = videoManifest.ValidationTime,
                    VideoId = videoManifest.Video.Id
                };
            }

            public class MetadataVideoSourceDto
            {
                public string Quality { get; set; } = default!;
                public string Reference { get; set; } = default!;
                public int? Size { get; set; }
                public int? Bitrate { get; set; }
            }

            public class SwarmImageRawDto
            {
                public int AspectRatio { get; set; }
                public string Blurhash { get; set; } = default!;
                public IReadOnlyDictionary<string, string> Sources { get; set; } = default!;
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

            VideoManifest = VideoManifestDto.FromManifestEntity(videoManifest);
        }

    }
}
