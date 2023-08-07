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
using static Etherna.EthernaIndex.Areas.Admin.Pages.UnsuitableVideoReports.UnsuitableReportModel.VideoReportDto;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.UnsuitableVideoReports
{
    public class UnsuitableReportModel : PageModel
    {
        public class VideoReportDto
        {
            public VideoReportDto(
                IEnumerable<VideoReportDetailDto> detailReports,
                string manifestHash,
                string manifestId,
                string title,
                string videoId)
            {
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));

                DetailReports = detailReports;
                ManifestHash = manifestHash;
                ManifestId = manifestId;
                Title = title;
                VideoId = videoId;
            }

            public IEnumerable<VideoReportDetailDto> DetailReports { get; }
            public string ManifestHash { get; private set; }
            public string ManifestId { get; set; } = default!;
            public string Title { get; private set; } = default!;
            public string VideoId { get; private set; }

            public class VideoReportDetailDto
            {
                public VideoReportDetailDto(
                    string id,
                    string reportAddress,
                    string reportDescription,
                    DateTime reportDate)
                {
                    if (id is null)
                        throw new ArgumentNullException(nameof(id));

                    Id = id;
                    Address = reportAddress;
                    Date = reportDate;
                    Description = reportDescription;
                    DetailType = UnsuitableReportDetailType.ReportVideo;
                }

                public VideoReportDetailDto(
                    string id,
                    string reviewerAddress,
                    string reviewDescription,
                    DateTime reviewDate,
                    bool isValid)
                {
                    if (id is null)
                        throw new ArgumentNullException(nameof(id));

                    Id = id;
                    Address = reviewerAddress;
                    Date = reviewDate;
                    Description = reviewDescription;
                    DetailType = UnsuitableReportDetailType.ManualVideoReview;
                    IsValid = isValid;
                }

                public string Id { get; }
                public string Address { get; }
                public string Description { get; }
                public DateTime Date { get; }
                public UnsuitableReportDetailType DetailType { get; }
                public bool IsValid { get; private set; }
            }

            public enum UnsuitableReportDetailType
            {
                ManualVideoReview,
                ReportVideo
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

            this.CustomRouteData = new Dictionary<string, string>();
            this.ethernaOidcClient = ethernaOidcClient;
            this.indexDbContext = indexDbContext;
            this.userService = userService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public Dictionary<string, string> CustomRouteData { get; private set; }
        public long MaxPage { get; private set; }
        public VideoReportDto VideoReport { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string videoId, int? p)
        {
            await InitializeAsync(videoId, p);
        }

        public async Task<IActionResult> OnPostApproveVideo(
            string videoId)
        {
            await CreateReviewAsync(videoId, true);

            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRejectVideo(
            string videoId)
        {
            await CreateReviewAsync(videoId, false);

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync(string videoId, int? p)
        {
            if (CustomRouteData.ContainsKey("videoId"))
                CustomRouteData["videoId"] = videoId;
            else
                CustomRouteData.Add("videoId", videoId);

            // Video info
            var video = await indexDbContext.Videos.FindOneAsync(vm => vm.Id == videoId);

            CurrentPage = p ?? 0;

            // Unsuitable video reports.
            var paginatedUnsuitableVideoReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                vm => vm.Where(i => i.Video.Id == video.Id),
                vm => vm.CreationDateTime,
                CurrentPage,
                PageSize);
            var videoReportDetails = paginatedUnsuitableVideoReports.Elements
                .Select(e => new VideoReportDetailDto(
                        e.Id,
                        e.ReporterAuthor.SharedInfoId,
                        e.Description,
                        e.CreationDateTime));
            MaxPage = paginatedUnsuitableVideoReports.MaxPage;

            // Manual video reviews.
            var paginatedManualVideoReviews = await indexDbContext.ManualVideoReviews.QueryPaginatedElementsAsync(
                vr => vr.Where(i => i.Video.Id == videoId),
                vr => vr.CreationDateTime,
                CurrentPage,
                PageSize);
            MaxPage += paginatedManualVideoReviews.MaxPage;
            videoReportDetails = videoReportDetails.Union(paginatedManualVideoReviews.Elements
                .Select(e => new VideoReportDetailDto(
                        e.Id,
                        e.Author.SharedInfoId,
                        e.Description,
                        e.CreationDateTime,
                        e.IsValidResult)));

            // Video report dto.
            var lastValidManifest = video.LastValidManifest;
            VideoReport = new VideoReportDto(
                videoReportDetails.Take(PageSize).OrderByDescending(i=>i.Date),
                lastValidManifest?.Manifest.Hash ?? "",
                lastValidManifest?.Id ?? "",
                lastValidManifest?.TryGetTitle() ?? "",
                video.Id);
        }

        private async Task CreateReviewAsync(string videoId, bool isValid)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, _) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            // Create ManualReview.
            await indexDbContext.ManualVideoReviews.CreateAsync(new ManualVideoReview(user, "", isValid, video));
        }
    }
}
