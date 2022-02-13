using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.Reviews
{
    public class IndexModel : PageModel
    {

        // Models.
        public class InputModel
        {
            [Required]
            [Display(Name = "Video Id")]
            public string VideoId { get; set; } = default!;
        }

        public class VideoReviewDto
        {
            public VideoReviewDto(
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
        private readonly IIndexDbContext dbContext;

        // Constructor.
        public IndexModel(
            IIndexDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
#pragma warning disable CA1002 // Do not expose generic lists
        public List<VideoReviewDto> VideoReports { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        // Methods.
        public async Task OnGetAsync(int? p)
        {
            await InitializeAsync(p);
        }

        // Helpers.
        private static IMongoQueryable<System.Linq.IGrouping<string, VideoReview>> FilterAndGroup(IMongoQueryable<VideoReview> elements)
        {
            return elements.GroupBy(i => i.Video.Id);
        }

        private async Task InitializeAsync(int? p)
        {
            CurrentPage = p ?? 0;

            // Count all distinct VideoReports.
            var totalReviews = await dbContext.VideoReviews.QueryElementsAsync(elements =>
                        FilterAndGroup(elements)
                        .CountAsync());

            // Get all VideoReports group by VideoId.
            var videoReviewsResult = await dbContext.VideoReviews.QueryElementsAsync(elements =>
                        FilterAndGroup(elements)
                        .OrderBy(i => i.Key)
                        .Skip(CurrentPage * PageSize)
                        .Take(PageSize)
                        .ToCursorAsync());

            var videoReviewsIds = new List<string>();
            while (await videoReviewsResult.MoveNextAsync())
            {
                foreach (var item in videoReviewsResult.Current)
                {
                    videoReviewsIds.Add(item.Key);
                }
            }

            // Get video and manifest info.
            var videos = await dbContext.Videos.QueryElementsAsync(elements =>
                elements.Where(u => videoReviewsIds.Contains(u.Id))
                        .OrderBy(i => i.Id)
                        .ToCursorAsync());
            while (await videos.MoveNextAsync())
            {
                foreach (var item in videos.Current)
                {
                    var manifest = item.GetLastValidManifest();
                    VideoReports.Add(new VideoReviewDto(manifest?.ManifestHash?.Hash ?? "", manifest?.Title ?? "", item.Id));
                }
            }

            MaxPage = totalReviews == 0 ? 0 : ((totalReviews + PageSize - 1) / PageSize) - 1;
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            var totalReviews = await dbContext.VideoReviews.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.Id == Input.VideoId) 
                        .CountAsync());

            if (totalReviews == 0)
            {
                ModelState.AddModelError(string.Empty, "Can't find report for video id");
                await InitializeAsync(p);
                return Page();
            }

            return RedirectToPage("History", new { videoId = Input.VideoId });
        }

    }
}
