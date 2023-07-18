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

using Etherna.Authentication;
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

namespace Etherna.EthernaIndex.Areas.Admin.Pages.UnsuitableVideoReports
{
    public class UnsuitableReportModel : PageModel
    {
        public class VideoReportDto
        {
            public VideoReportDto(
                IEnumerable<UnsuitableVideoReport> videoReports,
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
                    vr.ReporterAuthor.SharedInfoId,
                    vr.CreationDateTime));
                ManifestHash = manifestHash;
                ManifestId = manifestId;
                Title = title;
                VideoId = videoId;
                Reason = "";
            }

            public IEnumerable<VideoReportDetailDto> DetailReports { get; }
            public string ManifestHash { get; private set; }
            public string ManifestId { get; set; } = default!;
            public string Title { get; set; } = default!;
            public string VideoId { get; set; }
            public string Reason { get; set; }
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
                    ReporterAddress = reportAddress;
                    ReportDate = reportDate;
                }

                public string Id { get; }
                public string Description { get; }
                public string ReporterAddress { get; }
                public DateTime ReportDate { get; }
            }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly IUserService userService;

        // Constructor.
        public UnsuitableReportModel(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IIndexDbContext indexDbContext,
            IUserService userService)
        {
            if (indexDbContext is null)
                throw new ArgumentNullException(nameof(indexDbContext));
            this.ethernaOidcClient = ethernaOidcClient;
            this.indexDbContext = indexDbContext;
            this.userService = userService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public VideoReportDto VideoReport { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string videoId, int? p)
        {
            await InitializeAsync(videoId, p);
        }

        public async Task<IActionResult> OnPostApproveVideo(
            string videoId, string reason)
        {
            await CreateReviewAsync(videoId, reason, true);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRejectVideo(
            string videoId, string reason)
        {
            await CreateReviewAsync(videoId, reason, false);

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync(string videoId, int? p)
        {
            // Video info
            var video = await indexDbContext.Videos.FindOneAsync(vm => vm.Id == videoId);

            CurrentPage = p ?? 0;

            var paginatedHashVideoReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                vm => vm.Where(i => i.Video.Id == video.Id),
                vm => vm.CreationDateTime,
                CurrentPage,
                PageSize);

            MaxPage = paginatedHashVideoReports.MaxPage;

            var lastValidManifest = video.LastValidManifest;

            VideoReport = new VideoReportDto(
                paginatedHashVideoReports.Elements,
                lastValidManifest?.Manifest.Hash ?? "",
                lastValidManifest?.Id ?? "",
                lastValidManifest?.TryGetTitle() ?? "",
                video.Id);
        }

        private async Task CreateReviewAsync(string videoId, string reason, bool isValid)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, _) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            // Create ManualReview.
            await indexDbContext.ManualVideoReviews.CreateAsync(new ManualVideoReview(user, reason, isValid, video));
        }
    }
}
