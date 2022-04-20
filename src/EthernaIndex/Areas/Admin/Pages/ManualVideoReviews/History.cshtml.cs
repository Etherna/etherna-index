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
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Etherna.EthernaIndex.Areas.Admin.Pages.ManualVideoReviews.HistoryModel.VideoReviewDto;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.ManualVideoReviews
{
    public class HistoryModel : PageModel
    {
        public class VideoReviewDto
        {
            public VideoReviewDto(
                string manifestHash,
                string manifestId,
                string title,
                string videoId)
            {
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));
                if (manifestId is null)
                    throw new ArgumentNullException(nameof(manifestId));
                if (title is null)
                    throw new ArgumentNullException(nameof(title));
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                VideoId = videoId;
            }

            public string? ManifestHash { get; private set; }
            public string? ManifestId { get; set; }
            public string? Title { get; set; }
            public string VideoId { get; set; }

            public class VideoReviewDetailDto
            {
                public VideoReviewDetailDto(
                    string id,
                    string description,
                    bool isValid,
                    string reviewAddress,
                    DateTime reportDate)
                {
                    if (id is null)
                        throw new ArgumentNullException(nameof(id));

                    Id = id;
                    Description = description;
                    IsValid = isValid;
                    ReviewAddress = reviewAddress;
                    ReviewDate = reportDate;
                }

                public string Id { get; }
                public string Description { get; }
                public bool IsValid { get; }
                public string ReviewAddress { get; }
                public DateTime ReviewDate { get; }
            }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public HistoryModel(IIndexDbContext indexDbContext)
        {
            if (indexDbContext is null)
                throw new ArgumentNullException(nameof(indexDbContext));

            this.indexDbContext = indexDbContext;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public VideoReviewDto VideoReview { get; private set; } = default!;
        public IEnumerable<VideoReviewDetailDto> DetailReports { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(string videoId, int? p)
        {
            // Video info.
            var video = await indexDbContext.Videos.FindOneAsync(i => i.Id == videoId);

            // Get all reviews for video.
            CurrentPage = p ?? 0;

            var paginatedVideoManifests = await indexDbContext.ManualVideoReviews.QueryPaginatedElementsAsync(
                vr => vr.Where(i => i.Video.Id == videoId),
                vr => vr.CreationDateTime,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideoManifests.MaxPage;

            DetailReports = paginatedVideoManifests.Elements
                .Select(e => new VideoReviewDetailDto(
                        e.Id,
                        e.Description,
                        e.IsValidResult,
                        e.Author.SharedInfoId,
                        e.CreationDateTime));

            var currentManifest = video.LastValidManifest;

            VideoReview = new VideoReviewDto(
                currentManifest?.Manifest.Hash ?? "",
                currentManifest?.Id ?? "",
                currentManifest?.Title ?? "",
                video.Id);
        }
    }
}
