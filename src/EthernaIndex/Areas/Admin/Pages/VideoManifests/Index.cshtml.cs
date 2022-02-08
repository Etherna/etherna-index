using Etherna.EthernaIndex.Domain;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
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

        public class VideoManifestDto
        {
            public VideoManifestDto(
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
        public List<VideoManifestDto> VideoManifests { get; } = new();
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
            var totalManifests = await dbContext.VideoManifests.QueryElementsAsync(elements =>
                elements.CountAsync());

            //Get manifest info
            var videoManifests = await dbContext.VideoManifests.QueryElementsAsync(elements =>
                elements.OrderBy(i => i.Id)
                        .ToCursorAsync());
            while (await videoManifests.MoveNextAsync())
            {
                foreach (var itemManifest in videoManifests.Current)
                {
                    var title = itemManifest.Title ?? "";
                    VideoManifests.Add(new VideoManifestDto(itemManifest.ManifestHash.Hash, title, itemManifest.Video.Id));
                }
            }

            MaxPage = totalManifests == 0 ? 0 : ((totalManifests + PageSize - 1) / PageSize) - 1;
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            var totalVideo = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.LastCheck == null && //Only Report to check
                                    u.VideoManifest.ManifestHash.Hash == Input.ManifestHash) 
                        .CountAsync());

            if (totalVideo == 0)
            {
                ModelState.AddModelError(string.Empty, "Can't find manifest hash");
                await InitializeAsync(p);
                return Page();
            }

            return RedirectToPage("Manage", new { manifestHash = Input.ManifestHash });
        }

    }
}
