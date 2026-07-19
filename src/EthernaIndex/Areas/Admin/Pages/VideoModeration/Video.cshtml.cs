// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

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
    public class VideoModel(
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IIndexDbContext indexDbContext,
        IUserService userService)
        : PageModel
    {
        // Models.
        public abstract class HistoryElementBase
        {
            // Properties.
            public string Id { get; protected set; } = null!;
            public string AuthorSharedInfoId { get; protected set; } = null!;
            public string Description { get; protected set; } = null!;
            public DateTime CreationDateTime { get; protected set; }
        }
        
        public class InputModel
        {
            [Display(Name = "Reason")]
            public string Reason { get; set; } = null!;
        }
        
        public class ReportHistoryElement : HistoryElementBase
        {
            // Constructor.
            public ReportHistoryElement(UnsuitableVideoReport report)
            {
                ArgumentNullException.ThrowIfNull(report);

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
                ArgumentNullException.ThrowIfNull(review);

                Id = review.Id;
                AuthorSharedInfoId = review.Author.SharedInfoId;
                CreationDateTime = review.CreationDateTime;
                Description = review.Description;
                IsValid = review.IsValidResult;
            }

            // Properties.
            public bool IsValid { get; }
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = null!;
        public DateTime CreationDateTime { get; private set; }
        public IEnumerable<HistoryElementBase> HistoryElements { get; private set; } = null!;
        public bool IsFrozen { get; private set; }
        public VideoManifest? LastValidManifest { get; private set; }
        public string VideoId { get; private set; } = null!;

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
