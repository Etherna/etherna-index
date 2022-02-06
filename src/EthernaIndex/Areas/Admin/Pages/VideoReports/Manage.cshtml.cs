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
using Etherna.EthernaIndex.Services.Domain;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
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
        private readonly IIndexContext dbContext;
        private readonly IVideoReportService videoReportService;

        // Constructor.
        public ManageModel(
            IIndexContext dbContext,
            IVideoReportService videoReportService)
        {
            if (dbContext is null)
                throw new ArgumentNullException(nameof(dbContext));

            this.dbContext = dbContext;
            this.videoReportService = videoReportService;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public DateTime OperationDateTime { get; private set; } = default!;
        public VideoReportDto VideoReport { get; private set; } = default!;
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReportDetailDto> DetailReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(string manifestHash)
        {
            VideoReport = new VideoReportDto(manifestHash);
            await InitializeAsync();
        }

        public async Task<IActionResult> OnPostManageVideoReportAsync(
            string videoId,
            string button)
        {
            if (!ModelState.IsValid ||
                string.IsNullOrWhiteSpace(button))
                return RedirectToPage("Index");

            bool? isApproved;
            if (button.Equals("Approve Video", StringComparison.Ordinal))
            {
                isApproved = true;
            }
            else if (button.Equals("Reject Video", StringComparison.Ordinal))
            {
                isApproved = false;
            }
            else
            {
                return RedirectToPage("Index");
            }
                
            if (isApproved.Value)
            {
                await videoReportService.ApproveVideoAsync(videoId);
            }
            else
            {
                await videoReportService.RejectVideoAsync(videoId);
            }
                    
            await dbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync()
        {
            // Video info
            var videoManifest = await dbContext.VideoManifests.FindOneAsync(i => i.ManifestHash.Hash == VideoReport.ManifestHash);

            // Count all VideoReports.
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.VideoManifest.ManifestHash.Hash == VideoReport.ManifestHash &&
                                    u.LastCheck == null) //Only Report to check
                        .CountAsync());

            // Get all VideoReports paginated.
            var hashVideoReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.VideoManifest.ManifestHash.Hash == VideoReport.ManifestHash &&
                                    u.LastCheck == null) //Only Report to check
                                                         //.OrderBy(i => i.CreationDateTime)
                        .Skip(CurrentPage * PageSize)
                        .Take(PageSize)
                        .ToCursorAsync());

            var hashes = new List<string>();
            while (await hashVideoReports.MoveNextAsync())
            {
                foreach (var item in hashVideoReports.Current)
                {
                    DetailReports.Add(new VideoReportDetailDto(
                        item.Id,
                        item.ReportDescription,
                        item.ReporterOwner.Address,
                        item.CreationDateTime));
                }
            }

            VideoReport.ManifestId = videoManifest.Id;
            VideoReport.Title = videoManifest.Title ?? "";
            VideoReport.VideoId = videoManifest.Video.Id;

            MaxPage = totalVideo == 0 ? 0 : ((totalVideo + PageSize - 1) / PageSize) - 1;
        }

    }
}
