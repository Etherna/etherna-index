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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.UnsuitableVideoReports
{
    public class ReportModel : PageModel
    {

        // Models.
        public class InputModel
        {
            [Display(Name = "Include Reviewed")]
            public bool IncludeArchived { get; set; } = default!;

            [Display(Name = "Video Id")]
            public string? VideoId { get; set; }
        }

        public class VideoUnsuitableReportDto
        {
            public VideoUnsuitableReportDto(
                string title,
                string videoId,
                int totalReports)
            {
                if (title is null)
                    throw new ArgumentNullException(nameof(title));
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                Title = title;
                TotalReports = totalReports;
                VideoId = videoId;
            }

            public string Title { get; }
            public int TotalReports { get; }
            public string VideoId { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public ReportModel(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public IEnumerable<VideoUnsuitableReportDto> VideoUnsuitableReports { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(
            bool includeArchived, 
            string videoId,
            int? p)
        {
            await InitializeAsync(
                includeArchived,
                videoId,
                p);
        }

        public async Task OnPost()
        {
            await InitializeAsync(
                Input?.IncludeArchived ?? false,
                Input?.VideoId,
                null);
        }

        // Helpers.
        private async Task InitializeAsync(
            bool includeArchived,
            string? videoId,
            int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedUnsuitableReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                vm => VideoUnsuitableReportWhere(vm, includeArchived, videoId)
                        .GroupBy(i => i.Video.Id)
                        .Select(group => new
                        {
                            Id = group.Key,
                            Count = group.Count()
                        }),
                vm => vm.Id,
                CurrentPage,
                PageSize);

            MaxPage = paginatedUnsuitableReports.MaxPage;

            var videoIds = paginatedUnsuitableReports.Elements.Select(e => e.Id);

            // Get manifest info.
            var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
               elements.Where(u => videoIds.Contains(u.Id))
                       .OrderBy(i => i.Id)
                       .ToListAsync());

            VideoUnsuitableReports = videos.Select(vm => new VideoUnsuitableReportDto(
                vm.LastValidManifest?.TryGetTitle() ?? "",
                vm.Id,
                paginatedUnsuitableReports.Elements.First(pv => pv.Id == vm.Id).Count));
        }

        private IMongoQueryable<UnsuitableVideoReport> VideoUnsuitableReportWhere(
            IMongoQueryable<UnsuitableVideoReport> querable,
            bool includeArchived,
            string? videoId)
        {
            if (!string.IsNullOrWhiteSpace(videoId))
                querable = querable.Where(vur => vur.Video.Id == videoId);
            if (!includeArchived)
                querable = querable.Where(vur => !vur.IsArchived);

            return querable;
        }
    }
}
