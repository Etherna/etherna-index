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
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.ModerationVideos
{
    public partial class IndexModel : PageModel
    {

        // Models.
        public class VideoUnsuitableReportDto
        {
            public VideoUnsuitableReportDto(
                DateTime creationDateTime,
                string title,
                string videoId,
                int totalReports)
            {
                if (title is null)
                    throw new ArgumentNullException(nameof(title));
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                CreationDateTime = creationDateTime;
                Title = title;
                TotalReports = totalReports;
                VideoId = videoId;
            }

            public DateTime CreationDateTime { get; set; }
            public string Title { get; }
            public int TotalReports { get; }
            public string VideoId { get; }
        }

        // Consts.
        private const int PageSize = 20;

        [GeneratedRegex("^[A-Fa-f0-9]{24}$")]
        private static partial Regex VideoIdRegex();

        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public IndexModel(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
            ErrorMessage = "";
        }

        // Properties.
        public string ErrorMessage { get; private set; }
        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public IEnumerable<VideoUnsuitableReportDto> VideoUnsuitableReports { get; private set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(
            string? videoId,
            int? p)
        {
            return await InitializeAsync(
                videoId,
                p);
        }

        // Helpers.
        private async Task<IActionResult> InitializeAsync(
            string? videoId,
            int? p)
        {
            if (!string.IsNullOrWhiteSpace(videoId) &&
                !VideoIdRegex().IsMatch(videoId))
            {
                CurrentPage = 0;
                ErrorMessage = "Invalid VideoId format.";
                VideoUnsuitableReports = new List<VideoUnsuitableReportDto>();
                return new PageResult();
            }
            CurrentPage = p ?? 0;

            if (!string.IsNullOrWhiteSpace(videoId))
            {
                var video = await indexDbContext.Videos.TryFindOneAsync(v => v.Id == videoId);

                if (video is not null)
                    return RedirectToPage("../Videos/Index", new Dictionary<string, string> { { "videoId", video.Id } });
            }
            else
            {
                var paginatedUnsuitableReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                    vm => VideoUnsuitableReportWhere(vm, videoId)
                            .GroupBy(i => new { i.Video.Id, i.CreationDateTime })
                            .Select(group => new
                            {
                                Id = group.Key.Id,
                                CreationDateTime = group.Key.CreationDateTime,
                                Count = group.Count()
                            }),
                    vm => vm.CreationDateTime,
                    CurrentPage,
                    PageSize,
                    useDescendingOrder: true);

                MaxPage = paginatedUnsuitableReports.MaxPage;

                var videoIds = paginatedUnsuitableReports.Elements.Select(e => e.Id);

                // Get videos.
                var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
                   elements.Where(u => videoIds.Contains(u.Id))
                           .OrderBy(i => i.Id)
                           .ToListAsync());

                VideoUnsuitableReports = videos
                    .Select(v => new VideoUnsuitableReportDto(
                        v.CreationDateTime,
                        v.LastValidManifest?.TryGetTitle() ?? "",
                        v.Id,
                        paginatedUnsuitableReports.Elements.First(pv => pv.Id == v.Id).Count))
                    .OrderByDescending(vur => vur.CreationDateTime);
            }

            if (!string.IsNullOrWhiteSpace(videoId))
            {
                VideoUnsuitableReports = Array.Empty<VideoUnsuitableReportDto>();
                ErrorMessage = "VideoId not found.";
            }

            return new PageResult();
        }

        private IMongoQueryable<UnsuitableVideoReport> VideoUnsuitableReportWhere(
            IMongoQueryable<UnsuitableVideoReport> querable,
            string? videoId)
        {
            if (!string.IsNullOrWhiteSpace(videoId))
                querable = querable.Where(vur => vur.Video.Id == videoId);

            return querable;
        }
    }
}
