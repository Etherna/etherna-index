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
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public IndexModel(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
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

            var paginatedVideoManifests = await indexDbContext.VideoManifests.QueryPaginatedElementsAsync(
                vm => vm.Where(u => u.Video.ContentReview == null ||
                                    u.Video.ContentReview == ContentReviewType.WaitingReview)//Only Report to check
                        .GroupBy(i => i.Video.Id),
                vm => vm.Key,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideoManifests.MaxPage;

            var videoReportsIds = paginatedVideoManifests.Elements.Select(e => e.Key);

            // Get video and manifest info.
            var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
               elements.Where(u => videoReportsIds.Contains(u.Id))
                       .OrderBy(i => i.Id)
                       .ToListAsync());
            foreach (var item in videos)
            {
                var manifest = item.GetLastValidManifest();
                VideoReports.Add(new VideoReportDto(manifest?.ManifestHash?.Hash ?? "", manifest?.Title ?? "", item.Id));
            }
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            var totalVideo = await indexDbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.VideoManifest.ManifestHash.Hash == Input.ManifestHash)
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
