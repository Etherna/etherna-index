using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Etherna.Authentication;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoModeration
{
    public class VideoModel : PageModel
    {
        // Models.
        public abstract class HistoryElementBase
        {
            // Properties.
            public string Id { get; protected set; } = default!;
            public string AuthorSharedInfoId { get; protected set; } = default!;
            public string Description { get; protected set; } = default!;
            public DateTime CreationDateTime { get; protected set; }
        }
        
        public class InputModel
        {
            [Display(Name = "Reason")]
            public string Reason { get; set; } = default!;
        }
        
        public class ReportHistoryElement : HistoryElementBase
        {
            // Constructor.
            public ReportHistoryElement(UnsuitableVideoReport report)
            {
                if (report == null) throw new ArgumentNullException(nameof(report));

                Id = report.Id;
                AuthorSharedInfoId = report.ReporterAuthor.SharedInfoId;
                CreationDateTime = report.CreationDateTime;
                Description = report.Description;
            }
        }
        
        public class ReviewHistoryElement : HistoryElementBase
        {
            // Constructors.
            public ReviewHistoryElement(ManualVideoReview review)
            {
                if (review == null) throw new ArgumentNullException(nameof(review));

                Id = review.Id;
                AuthorSharedInfoId = review.Author.SharedInfoId;
                CreationDateTime = review.CreationDateTime;
                Description = review.Description;
                IsValid = review.IsValidResult;
            }

            // Properties.
            public bool IsValid { get; }
        }

        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly IUserService userService;

        // Constructor.
        public VideoModel(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IIndexDbContext indexDbContext,
            IUserService userService)
        {
            this.ethernaOidcClient = ethernaOidcClient;
            this.indexDbContext = indexDbContext;
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        public DateTime CreationDateTime { get; private set; }
        public IEnumerable<HistoryElementBase> HistoryElements { get; private set; } = default!;
        public bool IsFrozen { get; private set; }
        public VideoManifest? LastValidManifest { get; private set; }
        public string VideoId { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(
            string id)
        {
            VideoId = id;

            // Get video info
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == id);
            CreationDateTime = video.CreationDateTime;
            IsFrozen = video.IsFrozen;
            LastValidManifest = video.LastValidManifest;

            // Get history.
            var unsuitableVideoReports = await indexDbContext.UnsuitableVideoReports.QueryElementsAsync(
                elements => elements.Where(r => r.Video.Id == id)
                    .ToListAsync());

            var manualVideoReviews = await indexDbContext.ManualVideoReviews.QueryElementsAsync(
                elements => elements.Where(r => r.Video.Id == id)
                    .ToListAsync());

            HistoryElements = unsuitableVideoReports.Select(r => new ReportHistoryElement(r))
                .Union<HistoryElementBase>(manualVideoReviews.Select(r => new ReviewHistoryElement(r)))
                .OrderByDescending(historyElement => historyElement.CreationDateTime);
        }

        public async Task<IActionResult> OnPostApproveVideo(
            string id)
        {
            if (!ModelState.IsValid)
                return RedirectToPage(new { id });

            await CreateReviewAsync(id, true, Input.Reason);
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectVideo(
            string id)
        {
            if (!ModelState.IsValid)
                return RedirectToPage(new { id });

            await CreateReviewAsync(id, false, Input.Reason);
            return RedirectToPage(new { id });
        }

        // Helpers.
        private async Task CreateReviewAsync(string videoId, bool isValid, string reason)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, _) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            // Create ManualReview.
            await indexDbContext.ManualVideoReviews.CreateAsync(new ManualVideoReview(user, reason, isValid, video));
        }
    }
}
