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
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.SwarmSdk.Models;
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
        // Models.
        public class VideoManifestDto
        {
            // Constructor.
            public VideoManifestDto(
                Video? video,
                VideoManifest videoManifest)
            {
                ArgumentNullException.ThrowIfNull(videoManifest);

                CreationDateTime = videoManifest.CreationDateTime;
                Description = videoManifest.TryGetDescription();
                ErrorsDetails = videoManifest.ValidationErrors.Select(i => $"[{i.ErrorType}]: {i.ErrorMessage}");
                IsValid = videoManifest.IsValid;
                ManifestReference = videoManifest.ManifestReference;
                Title = videoManifest.TryGetTitle() ?? videoManifest.Id;
                ValidationTime = videoManifest.ValidationTime;
                VideoInfo = video is null ? null : new VideoInfoDto(video);
            }

            // Properties.
            public DateTime CreationDateTime { get; set; }
            public string? Description { get; set; }
            public IEnumerable<string> ErrorsDetails { get; set; }
            public bool? IsValid { get; set; }
            public SwarmReference ManifestReference { get; set; }
            public string? Title { get; set; }
            public DateTime? ValidationTime { get; set; }
            public VideoInfoDto? VideoInfo { get; set; }
        }

        public class VideoInfoDto(Video video)
        {
            public string VideoId { get; set; } = video.Id;
        }

        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public ManifestModel(
            IBackgroundJobClient backgroundJobClient,
            IIndexDbContext indexDbContext)
        {
            ArgumentNullException.ThrowIfNull(indexDbContext);

            this.backgroundJobClient = backgroundJobClient;
            this.indexDbContext = indexDbContext;
        }

        // Properties.
        public VideoManifestDto VideoManifest { get; private set; } = null!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(SwarmReference manifestReference)
        {
            // Video info
            var videoManifest = await indexDbContext.VideoManifests.TryFindOneAsync(vm => vm.ManifestReference == manifestReference);
            if (videoManifest is null)
                return RedirectToPage("Index", new { manifestReference }); //the search reports the unknown reference

            var video = await indexDbContext.Videos.TryFindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            VideoManifest = new VideoManifestDto(video, videoManifest);
            return Page();
        }

        public async Task<IActionResult> OnPostForceNewValidationAsync(SwarmReference manifestReference)
        {
            // Get Manifest & Video data.
            var videoManifest = await indexDbContext.VideoManifests.TryFindOneAsync(vm => vm.ManifestReference == manifestReference);
            if (videoManifest is null)
                return RedirectToPage("Index", new { manifestReference }); //the search reports the unknown reference

            var video = await indexDbContext.Videos.TryFindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));
            if (video is null)
                return RedirectToPage(new { manifestReference }); //an orphan manifest has no video to validate for

            // Background Validator.
            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, videoManifest.ManifestReference.ToString()),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            return RedirectToPage("Index");
        }
    }
}
