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
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoModeration
{
    public class IndexModel(IIndexDbContext indexDbContext) : PageModel
    {
        // Models.
        public class VideoReportsAggregateDto(
            int totalReports,
            string videoId,
            DateTime? videoCreationDateTime,
            string? videoTitle)
        {
            public int TotalReports { get; } = totalReports;
            public DateTime? VideoCreationDateTime { get; } = videoCreationDateTime;
            public string VideoId { get; } = videoId ?? throw new ArgumentNullException(nameof(videoId));
            public string? VideoTitle { get; } = videoTitle;
        }

        // Consts.
        private const int PageSize = 20;

        // Properties.
        public string ErrorMessage { get; private set; } = "";
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public IEnumerable<VideoReportsAggregateDto> VideoUnsuitableReports { get; private set; } = null!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(
            string? videoId,
            int? p)
        {
            CurrentPage = p ?? 0;

            if (!string.IsNullOrWhiteSpace(videoId))
            {
                var video = await indexDbContext.Videos.TryFindOneAsync(v => v.Id == videoId);

                if (video is not null)
                    return RedirectToPage("Video", new { video.Id });
                
                VideoUnsuitableReports = Array.Empty<VideoReportsAggregateDto>();
                ErrorMessage = "VideoId not found.";
            }
            else
            {
                var paginatedReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                    reports => reports
                        .Where(r => !r.IsArchived)
                        .GroupBy(r => r.Video.Id)
                        .Select(group => new
                        {
                            VideoId = group.Key,
                            Count = group.Count()
                        }),
                    reportsAggregate => reportsAggregate.Count,
                    CurrentPage,
                    PageSize,
                    useDescendingOrder: true);

                MaxPage = paginatedReports.MaxPage;

                // Get video info.
                var videoIds = paginatedReports.Elements.Select(e => e.VideoId);
                var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
                   elements.Where(v => videoIds.Contains(v.Id))
                           .ToListAsync());
                await indexDbContext.LoadValuesAsync(
                    videos.Select(v => v.LastValidManifest).OfType<VideoManifest>(),
                    m => m.Metadata);

                VideoUnsuitableReports = paginatedReports.Elements.Select(r =>
                {
                    var video = videos.FirstOrDefault(v => v.Id == r.VideoId);
                    return new VideoReportsAggregateDto(
                        r.Count,
                        r.VideoId,
                        video?.CreationDateTime,
                        video?.LastValidManifest?.TryGetTitle());
                });
            }

            return new PageResult();
        }
    }
}
