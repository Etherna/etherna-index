using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class IndexModel : PageModel
    {

        // Models.
        public class InputModel
        {
            [Display(Name = "Manifest Hash")]
            public string? ManifestHash { get; set; }

            [Display(Name = "Video Id")]
            public string? VideoId { get; set; }
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
        public IEnumerable<VideoManifestDto> VideoManifests { get; set; } = default!;

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
                vm => VideoWhere(vm),
                vm => vm.Id,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideoManifests.MaxPage;

            VideoManifests= paginatedVideoManifests.Elements.Select( 
                e => new VideoManifestDto(
                    e.ManifestHash.Hash, 
                    e.Title ?? "", 
                    e.Video.Id));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitializeAsync(null);

            return Page();
        }

        private IMongoQueryable<VideoManifest> VideoWhere(IMongoQueryable<VideoManifest> querable)
        {
            if (!string.IsNullOrWhiteSpace(Input?.ManifestHash))
                querable = querable.Where(v => v.ManifestHash.Hash == Input.ManifestHash);
            if (!string.IsNullOrWhiteSpace(Input?.VideoId))
                querable = querable.Where(v => v.Video.Id == Input.VideoId);

            return querable;
        }

    }
}
