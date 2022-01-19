using Etherna.EthernaIndex.Domain;
using Etherna.MongoDB.Driver.Linq;
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

        // Constructor.
        public ManageModel(
            IIndexContext dbContext)
        {
            if (dbContext is null)
                throw new ArgumentNullException(nameof(dbContext));

            this.dbContext = dbContext;
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public string HashReportVideo { get; private set; } = default!;
        public int MaxPage { get; private set; }
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReportDetailDto> VideoReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(string hash)
        {
            HashReportVideo = hash;
            await InitializeAsync(hash);
        }

        // Helpers.
        private async Task InitializeAsync(string hash)
        {
            //Count all VideoReports
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.ManifestHash.Hash == hash &&
                                    u.LastCheck == null) //Only Report to check
                        .CountAsync());

            //Get all VideoReports paginated
            var hashVideoReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.ManifestHash.Hash == hash &&
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

            MaxPage = ((totalVideo + PageSize - 1) / PageSize) - 1;
        }

    }
}
