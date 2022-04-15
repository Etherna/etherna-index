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

namespace Etherna.EthernaIndex.Areas.Admin.Pages.ManualVideoReviews
{
    public class IndexModel : PageModel
    {
        // Models.
        public class InputModel
        {
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
        public async Task OnGetAsync(
            string? videoId,
            int? p)
        {
            await InitializeAsync(
                videoId,
                p);
        }

        public async Task OnPost()
        {
            await InitializeAsync(
                Input?.VideoId,
                null);
        }

        // Helpers.
        private async Task InitializeAsync(
            string? videoId,
            int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedVideoReviews = await indexDbContext.ManualVideoReviews.QueryPaginatedElementsAsync(
                vm => VideoUnsuitableReviewWhere(vm, videoId)
                        .GroupBy(i => i.Video.Id)
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
                var manifest = v.LastValidManifest;
                return new VideoReviewDto(manifest?.Manifest?.Hash ?? "", manifest?.Title ?? "", v.Id);
            });
        }

        private IMongoQueryable<ManualVideoReview> VideoUnsuitableReviewWhere(
            IMongoQueryable<ManualVideoReview> querable,
            string? videoId)
        {
            if (!string.IsNullOrWhiteSpace(videoId))
                querable = querable.Where(vur => vur.Video.Id == videoId);

            return querable;
        }

    }
}
