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
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoReviews
{
    public class ManageModel : PageModel
    {
        public class VideoReviewDto
        {
            public VideoReviewDto(
                string videoId)
            {
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                VideoId = videoId;
            }

            public string? ManifestHash { get; private set; }
            public string? ManifestId { get; set; }
            public string? Title { get; set; }
            public string VideoId { get; set; }
        }

        public class VideoReviewDetailDto
        {
            public VideoReviewDetailDto(
                string id,
                ContentReviewType contentReview,
                string description,
                string reviewAddress,
                DateTime reportDate)
            {
                if (id is null)
                    throw new ArgumentNullException(nameof(id));

                Id = id;
                ContentReview = contentReview;
                Description = description;
                ReviewAddress = reviewAddress;
                ReviewDate = reportDate;
            }

            public string Id { get; }
            public ContentReviewType ContentReview { get; }
            public string Description { get; }
            public string ReviewAddress { get; }
            public DateTime ReviewDate { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIndexDbContext dbContext;
        private readonly IVideoReportService videoReportService;

        // Constructor.
        public ManageModel(
            IHttpContextAccessor httpContextAccessor,
            IIndexDbContext dbContext,
            IVideoReportService videoReportService)
        {
            if (dbContext is null)
                throw new ArgumentNullException(nameof(dbContext));

            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
            this.videoReportService = videoReportService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public VideoReviewDto VideoReview { get; private set; } = default!;
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReviewDetailDto> DetailReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(string videoId)
        {
            VideoReview = new VideoReviewDto(videoId);
            await InitializeAsync();
        }

        // Helpers.
        private async Task InitializeAsync()
        {
            // Video info
            var video = await dbContext.Videos.FindOneAsync(i => i.Id == VideoReview.VideoId);

            // Count all VideoReview.
            var totalReviews = await dbContext.VideoReviews.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.Id == VideoReview.VideoId)
                        .CountAsync());

            // Get all VideoReports paginated.
            var hashVideoReports = await dbContext.VideoReviews.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.Id == VideoReview.VideoId) //Only Report to check
                        .OrderBy(i => i.CreationDateTime)
                        .Skip(CurrentPage * PageSize)
                        .Take(PageSize)
                        .ToCursorAsync());

            var hashes = new List<string>();
            while (await hashVideoReports.MoveNextAsync())
            {
                foreach (var item in hashVideoReports.Current)
                {
                    DetailReports.Add(new VideoReviewDetailDto(
                        item.Id,
                        item.ContentReview,
                        item.Description,
                        item.ReviewOwner.Address,
                        item.CreationDateTime));
                }
            }

            var currentManifest = video.GetLastValidManifest();
            VideoReview.ManifestId = currentManifest?.Id ?? "";
            VideoReview.Title = currentManifest?.Title ?? "";
            VideoReview.VideoId = video.Id;

            MaxPage = totalReviews == 0 ? 0 : ((totalReviews + PageSize - 1) / PageSize) - 1;
        }

    }
}
