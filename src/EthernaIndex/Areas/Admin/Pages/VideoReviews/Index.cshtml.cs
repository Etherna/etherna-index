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

namespace Etherna.EthernaIndex.Areas.Admin.Pages.Reviews
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

        public class VideoReviewDto
        {
            public VideoReviewDto(
                string lastValidManifestHash,
                string title,
                string videoId)
            {
                if (lastValidManifestHash is null)
                    throw new ArgumentNullException(nameof(lastValidManifestHash));
                if (title is null)
                    throw new ArgumentNullException(nameof(title));
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                LastValidManifestHash = lastValidManifestHash;
                Title = title;
                VideoId = videoId;
            }

            public string LastValidManifestHash { get; }
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
        public IEnumerable<VideoReviewDto> VideoReports { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(int? p)
        {
            await InitializeAsync(p);
        }

        // Helpers.
        private async Task InitializeAsync(int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedVideoReviews = await indexDbContext.VideoReviews.QueryPaginatedElementsAsync(
                vm => VideoReviewsWhere(vm)
                        .GroupBy(i => i.VideoId)
                        .Select(group => new
                        {
                            Id = group.Key,
                            Count = group.Count()
                        }),
                vm => vm.Id,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideoReviews.MaxPage;

            var videoReviewsIds = paginatedVideoReviews.Elements.Select(e => e.Id);

            // Get video and manifest info.
            var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
                elements.Where(u => videoReviewsIds.Contains(u.Id))
                        .OrderBy(i => i.Id)
                        .ToListAsync());

            VideoReports = videos.Select(v =>
            {
                var manifest = v.GetLastValidManifest();
                return new VideoReviewDto(manifest?.ManifestHash?.Hash ?? "", manifest?.Title ?? "", v.Id);
            });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitializeAsync(null);

            return Page();
        }

        private IMongoQueryable<VideoReview> VideoReviewsWhere(IMongoQueryable<VideoReview> querable)
        {
            if (!string.IsNullOrWhiteSpace(Input?.ManifestHash))
                querable = querable.Where(vr => vr.ManifestHash == Input.ManifestHash);
            if (!string.IsNullOrWhiteSpace(Input?.VideoId))
                querable = querable.Where(vr => vr.VideoId == Input.VideoId);

            return querable;
        }

    }
}
