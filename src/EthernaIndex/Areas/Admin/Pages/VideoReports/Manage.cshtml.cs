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
        public string HashReportVideo { get; private set; } = default!;
        public int MaxPage { get; private set; }
        public DateTime OperationDateTime { get; private set; } = default!;
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReportDetailDto> VideoReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(string hash)
        {
            HashReportVideo = hash;
            await InitializeAsync(hash);
        }

        public async Task<IActionResult> OnPostManageVideoReportAsync(
            string hashReportVideo,
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
                await videoReportService.ApproveVideoAsync(hashReportVideo);
            }
            else
            {
                await videoReportService.RejectVideoAsync(hashReportVideo);
            }
                    
            await dbContext.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        // Helpers.
        private async Task InitializeAsync(string hash)
        {
            //Count all VideoReports
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.Id == hash &&
                                    u.LastCheck == null) //Only Report to check
                        .CountAsync());

            //Get all VideoReports paginated
            var hashVideoReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.Id == hash &&
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
                    VideoReports.Add(new VideoReportDetailDto(
                        item.Id,
                        item.ReportDescription,
                        item.ReporterOwner.Address,
                        item.CreationDateTime));
                }
            }

            MaxPage = totalVideo == 0 ? 0 : ((totalVideo + PageSize - 1) / PageSize) - 1;
        }

    }
}
