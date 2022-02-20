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
    public class ReportModel : PageModel
    {

        // Models.
        public class InputModel
        {
            [Display(Name = "Manifest Hash")]
            public string? ManifestHash { get; set; }

            [Display(Name = "Video Id")]
            public string? VideoId { get; set; }

            [Display(Name = "Include Reviewed")]
            public bool IncludeReportReviewed { get; set; } = default!;
        }

        public class VideoReportDto
        {
            public VideoReportDto(
                string manifestHash,
                string title,
                string videoId,
                int totalReports)
            {
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));
                if (title is null)
                    throw new ArgumentNullException(nameof(title));
                if (videoId is null)
                    throw new ArgumentNullException(nameof(videoId));

                ManifestHash = manifestHash;
                Title = title;
                TotalReports = totalReports;
                VideoId = videoId;
            }

            public string ManifestHash { get; }
            public string Title { get; }
            public int TotalReports { get; }
            public string VideoId { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public ReportModel(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public int MaxPage { get; private set; }
        public IEnumerable<VideoReportDto> VideoReports { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(int? p)
        {
            await InitializeAsync(p);
        }

        // Helpers.
        private async Task InitializeAsync(int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedVideos = await indexDbContext.VideoReports.QueryPaginatedElementsAsync(
                vm => VideoReportsWhere(vm)
                        .GroupBy(i => i.VideoManifest.Id)
                        .Select(group => new
                        {
                            Id = group.Key,
                            Count = group.Count()
                        }),
                vm => vm.Id,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideos.MaxPage;

            var videoManifestIds = paginatedVideos.Elements.Select(e => e.Id);

            // Get manifest info.
            var videoManifests = await indexDbContext.VideoManifests.QueryElementsAsync(elements =>
               elements.Where(u => videoManifestIds.Contains(u.Id))
                       .OrderBy(i => i.Id)
                       .ToListAsync());

                VideoReports = videoManifests.Select(vm => new VideoReportDto(
                    vm.ManifestHash.Hash,
                    vm.Title ?? "",
                    vm.Video.Id,
                    paginatedVideos.Elements.First(pv => pv.Id == vm.Id).Count));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitializeAsync(null);

            return Page();
        }

        private IMongoQueryable<VideoReport> VideoReportsWhere(IMongoQueryable<VideoReport> querable)
        {
            if (!string.IsNullOrWhiteSpace(Input?.ManifestHash))
                querable = querable.Where(vr => vr.VideoManifest.ManifestHash.Hash == Input.ManifestHash);
            if (!string.IsNullOrWhiteSpace(Input?.VideoId))
                querable = querable.Where(vr => vr.VideoManifest.Video.Id == Input.VideoId);
            if (Input is null || 
                !Input.IncludeReportReviewed)
                return querable.Where(vr => vr.VideoManifest.ReviewApproved == null);

            return querable;
        }

    }
}
