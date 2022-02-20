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
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoReports
{
    public class ManageModel : PageModel
    {
        public class VideoReportDto
        {
            public VideoReportDto(
                IEnumerable<VideoReport> videoReports,
                bool hasOtherValidManifest,
                string manifestHash,
                string manifestId,
                string title,
                string videoId)
            {
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));

                DetailReports = videoReports.Select(vr => new VideoReportDetailDto(
                    vr.Id,
                    vr.Description,
                    vr.VideoManifest.ManifestHash.Hash,
                    vr.ReporterAuthor.SharedInfoId,
                    vr.CreationDateTime));
                ManifestHash = manifestHash;
                HasOtherValidManifest = hasOtherValidManifest;
                ManifestId = manifestId;
                Title = title;
                VideoId = videoId;
            }

            public IEnumerable<VideoReportDetailDto> DetailReports { get; }
            public bool HasOtherValidManifest { get; set; }
            public string ManifestHash { get; private set; }
            public string ManifestId { get; set; } = default!;
            public string Title { get; set; } = default!;
            public string VideoId { get; set; }
            public class VideoReportDetailDto
            {
                public VideoReportDetailDto(
                    string id,
                    string description,
                    string manifestHash,
                    string reportAddress,
                    DateTime reportDate)
                {
                    if (id is null)
                        throw new ArgumentNullException(nameof(id));

                    Id = id;
                    Description = description;
                    ManifestHash = manifestHash;
                    ReporterAddress = reportAddress;
                    ReportDate = reportDate;
                }

                public string Id { get; }
                public string Description { get; }
                public string ManifestHash { get; }
                public string ReporterAddress { get; }
                public DateTime ReportDate { get; }
            }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;
        private readonly IUserService userService;
        private readonly IVideoReportService videoReportService;

        // Constructor.
        public ManageModel(
            IIndexDbContext indexDbContext,
            IUserService userService,
            IVideoReportService videoReportService)
        {
            if (indexDbContext is null)
                throw new ArgumentNullException(nameof(indexDbContext));

            this.indexDbContext = indexDbContext;
            this.userService = userService;
            this.videoReportService = videoReportService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public VideoReportDto VideoReport { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string videoId, string manifestHash, int? p)
        {
            await InitializeAsync(videoId, manifestHash, p);
        }

        public async Task<IActionResult> OnPostApprovedManifest(
            string videoId,
            string manifestHash)
        {
            await SetReviewAsync(videoId, manifestHash, ContentReviewStatus.ApprovedManifest);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRejectManifest(
            string videoId,
            string manifestHash)
        {
            await SetReviewAsync(videoId, manifestHash, ContentReviewStatus.RejectManifest);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRejectVideo(
            string videoId,
            string manifestHash)
        {
            await SetReviewAsync(videoId, manifestHash, ContentReviewStatus.RejectVideo);

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync(string videoId, string manifestHash, int? p)
        {
            // Video info
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.ManifestHash.Hash == manifestHash &&
                                                                                        vm.Video.Id == videoId);
            var video = await indexDbContext.Videos.FindOneAsync(vm => vm.Id == videoId);

            CurrentPage = p ?? 0;

            var paginatedHashVideoReports = await indexDbContext.VideoReports.QueryPaginatedElementsAsync(
                vm => vm.Where(i => i.VideoManifest.ManifestHash.Hash == videoManifest.ManifestHash.Hash),
                vm => vm.CreationDateTime,
                CurrentPage,
                PageSize);

            MaxPage = paginatedHashVideoReports.MaxPage;

            var hasOtherValidManifest = video.VideoManifests.Any(vm => vm.IsValid == true &&
                                                                    vm.ManifestHash.Hash != videoManifest.ManifestHash.Hash);

            VideoReport = new VideoReportDto(
                paginatedHashVideoReports.Elements,
                hasOtherValidManifest,
                videoManifest.ManifestHash.Hash,
                videoManifest.Id,
                videoManifest.Title ?? "",
                video.Id);
        }

        private async Task SetReviewAsync(string videoId, string manifestHash, ContentReviewStatus contentReviewStatus)
        {
            var address = HttpContext!.User.GetEtherAddress();
            var (user, _) = await userService.FindUserAsync(address);

            await videoReportService.SetReviewAsync(videoId, manifestHash, contentReviewStatus, user, "");
        }

    }
}
