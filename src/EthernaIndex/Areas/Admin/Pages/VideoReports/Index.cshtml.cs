using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nethereum.Util;
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
            [Display(Name = "Video hash")]
            public string ManifestHash { get; set; } = default!;
        }

        public class VideoReportDto
        {
            public VideoReportDto(string id)
            {
                if (id is null)
                    throw new ArgumentNullException(nameof(id));

                Id = id;
            }

            public string Id { get; }
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

            //Count all Videos
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null) //Only Report to check
                        .GroupBy(i => i.Video.ManifestHash.Hash)
                        .CountAsync());

            //Get all VideoReports paginated by Hash
            var hashVideoReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null) //Only Report to check
                        .GroupBy(i => i.Video.ManifestHash.Hash)
                        .OrderBy(i => i.Key)
                        .Skip(CurrentPage * PageSize)
                        .Take(PageSize)
                        .ToCursorAsync());

            var hashes = new List<string>();
            while (await hashVideoReports.MoveNextAsync())
            {
                foreach (var item in hashVideoReports.Current)
                {
                    hashes.Add(item.Key);
                }
            }

            //Get video info
            var videos = await dbContext.Videos.QueryElementsAsync(elements =>
                elements.Where(u => hashes.Contains(u.ManifestHash.Hash)) //Only Report to check
                        .OrderBy(i => i.ManifestHash.Hash)
                        .ToCursorAsync());
            while (await videos.MoveNextAsync())
            {
                foreach (var item in videos.Current)
                {
                    VideoReports.Add(new VideoReportDto(item.ManifestHash.Hash));
                }
            }

            MaxPage = totalVideo == 0 ? 0 : ((totalVideo + PageSize - 1) / PageSize) - 1;
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null && //Only Report to check
                                    u.Video.ManifestHash.Hash == Input.ManifestHash) 
                        .CountAsync());

            if (totalVideo == 0)
            {
                ModelState.AddModelError(string.Empty, "Can't find report for video hash");
                await InitializeAsync(p);
                return Page();
            }

            return RedirectToPage("Manage", new { hash = Input.ManifestHash });
        }


    }
}
