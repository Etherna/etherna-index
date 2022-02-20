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
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Etherna.EthernaIndex.Areas.Admin.Pages.VideoReviews.ManageModel.VideoReviewDto;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoReviews
{
    public class ManageModel : PageModel
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
                    ContentReviewStatus contentReview,
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
                public ContentReviewStatus ContentReview { get; }
                public string Description { get; }
                public string ReviewAddress { get; }
                public DateTime ReviewDate { get; }
            }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public ManageModel(IIndexDbContext indexDbContext)
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
            await InitializeAsync(videoId, p);
        }

        // Helpers.
        private async Task InitializeAsync(string videoId, int? p)
        {
            // Video info.
            var video = await indexDbContext.Videos.FindOneAsync(i => i.Id == videoId);

            // Get all reviews for video.
            CurrentPage = p ?? 0;

            var paginatedVideoManifests = await indexDbContext.VideoReviews.QueryPaginatedElementsAsync(
                vr => vr.Where(i => i.VideoId == videoId),
                vr => vr.CreationDateTime,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideoManifests.MaxPage;

            DetailReports = paginatedVideoManifests.Elements
                .Select(e => new VideoReviewDetailDto(
                        e.Id,
                        e.ContentReview,
                        e.Description,
                        e.ReviewAuthor.SharedInfoId,
                        e.CreationDateTime));

            var currentManifest = video.GetLastValidManifest();

            VideoReview = new VideoReviewDto(
                currentManifest?.ManifestHash.Hash ?? "",
                currentManifest?.Id ?? "",
                currentManifest?.Title ?? "",
                video.Id);
        }

    }
}
