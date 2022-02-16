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
using Microsoft.AspNetCore.Http;
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
                string manifestHash)
            {
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));

                ManifestHash = manifestHash;
            }

            public string ManifestHash { get; private set; }
            public string ManifestId { get; set; } = default!;
            public string Title { get; set; } = default!;
            public string VideoId { get; set; } = default!;
        }

        public class VideoReportDetailDto
        {
            public VideoReportDetailDto(
                string id,
                string description,
                string reportAddress,
                DateTime reportDate)
            {
                if (id is null)
                    throw new ArgumentNullException(nameof(id));

                Id = id;
                Description = description;
                ReportAddress = reportAddress;
                ReportDate = reportDate;
            }

            public string Id { get; }
            public string Description { get; }
            public string ReportAddress { get; }
            public DateTime ReportDate { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;
        private readonly IVideoReportService videoReportService;

        // Constructor.
        public ManageModel(
            IIndexDbContext indexDbContext,
            IVideoReportService videoReportService)
        {
            if (indexDbContext is null)
                throw new ArgumentNullException(nameof(indexDbContext));

            this.indexDbContext = indexDbContext;
            this.videoReportService = videoReportService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public VideoReportDto VideoReport { get; private set; } = default!;
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReportDetailDto> DetailReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(string manifestHash, int? p)
        {
            VideoReport = new VideoReportDto(manifestHash);
            await InitializeAsync(p);
        }

        public async Task<IActionResult> OnPostManageVideoReportAsync(
            string videoId,
            string manifestHash,
            string button)
        {
            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(button))
                return RedirectToPage("Index");

            ContentReviewType contentReviewType;
            if (button.Equals("Approve Video", StringComparison.Ordinal))
                contentReviewType = ContentReviewType.ApprovedVideo;
            else if (button.Equals("Reject Video", StringComparison.Ordinal))
                contentReviewType = ContentReviewType.RejectVideo;
            else if (button.Equals("Approve Manifest", StringComparison.Ordinal))
                contentReviewType = ContentReviewType.ApprovedManifest;
            else if (button.Equals("Reject Manifest", StringComparison.Ordinal))
                contentReviewType = ContentReviewType.RejectManifest;
            else if (button.Equals("Waiting Review", StringComparison.Ordinal))
                contentReviewType = ContentReviewType.WaitingReview;
            else
                return RedirectToPage("Index");

            var address = HttpContext.User.GetEtherAddress();
            var user = await indexDbContext.Users.FindOneAsync(c => c.Address == address);

            await videoReportService.SetReviewAsync(videoId, manifestHash, contentReviewType, user);

            await indexDbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync(int? p)
        {
            // Video info
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(i => i.ManifestHash.Hash == VideoReport.ManifestHash);

            CurrentPage = p ?? 0;

            var paginatedHashVideoReports = await indexDbContext.VideoReports.QueryPaginatedElementsAsync(
                vm => vm.Where(i => i.VideoManifest.ManifestHash.Hash == VideoReport.ManifestHash),
                vm => vm.CreationDateTime,
                CurrentPage,
                PageSize);

            MaxPage = paginatedHashVideoReports.MaxPage;

            DetailReports.AddRange(paginatedHashVideoReports.Elements
                .Select(vr => new VideoReportDetailDto(
                    vr.Id,
                    vr.Description,
                    vr.ReporterOwner.Address,
                    vr.CreationDateTime)));

            VideoReport.ManifestId = videoManifest.Id;
            VideoReport.Title = videoManifest.Title ?? "";
            VideoReport.VideoId = videoManifest.Video.Id;
        }

    }
}
