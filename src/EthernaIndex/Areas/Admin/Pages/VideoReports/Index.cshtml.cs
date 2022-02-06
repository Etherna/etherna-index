using Etherna.EthernaIndex.Domain;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoReports
{
    public class IndexModel : PageModel
    {

        // Models.
        public class InputModel
        {
            [Required]
            [Display(Name = "Manifest Hash")]
            public string ManifestHash { get; set; } = default!;
        }

        public class VideoReportDto
        {
            public VideoReportDto(
                string manifestHash,
                string title,
                string videoId)
            {
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));
                if (title is null)
                    throw new ArgumentNullException(nameof(title));
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                ManifestHash = manifestHash;
                Title = title;
                VideoId = videoId;
            }

            public string ManifestHash { get; }
            public string Title { get; }
            public string VideoId { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexContext dbContext;

        // Constructor.
        public IndexModel(
            IIndexContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReportDto> VideoReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(int? p)
        {
            await InitializeAsync(p);
        }

        // Helpers.
        private async Task InitializeAsync(int? p)
        {
            CurrentPage = p ?? 0;

            // Count all distinct reports.
            var totalReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null) //Only Report to check
                        .GroupBy(i => i.VideoManifest.Id)
                        .CountAsync());

            // Get all VideoReports group by Hash.
            var videoReportsResult = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null) //Only Report to check
                        .GroupBy(i => i.VideoManifest.Id)
                        .OrderBy(i => i.Key)
                        .Skip(CurrentPage * PageSize)
                        .Take(PageSize)
                        .ToCursorAsync());

            var videoReportsIds = new List<string>();
            while (await videoReportsResult.MoveNextAsync())
            {
                foreach (var item in videoReportsResult.Current)
                {
                    videoReportsIds.Add(item.Key);
                }
            }

            //Get manifest info
            var videoManifests = await dbContext.VideoManifests.QueryElementsAsync(elements =>
                elements.Where(u => videoReportsIds.Contains(u.Id))
                        .OrderBy(i => i.Id)
                        .ToCursorAsync());
            while (await videoManifests.MoveNextAsync())
            {
                foreach (var itemManifest in videoManifests.Current)
                {
                    var title = itemManifest.Title ?? "";
                    VideoReports.Add(new VideoReportDto(itemManifest.ManifestHash.Hash, title, itemManifest.Video.Id));
                }
            }

            MaxPage = totalReports == 0 ? 0 : ((totalReports + PageSize - 1) / PageSize) - 1;
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null && //Only Report to check
                                    u.VideoManifest.ManifestHash.Hash == Input.ManifestHash) 
                        .CountAsync());

            if (totalVideo == 0)
            {
                ModelState.AddModelError(string.Empty, "Can't find report for video hash");
                await InitializeAsync(p);
                return Page();
            }

            return RedirectToPage("Manage", new { manifestHash = Input.ManifestHash });
        }

    }
}
