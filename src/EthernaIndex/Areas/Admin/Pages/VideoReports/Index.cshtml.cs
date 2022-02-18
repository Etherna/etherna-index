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
            foreach (var videoManifest in videoManifests)
                VideoReports.Add(
                    new VideoReportDto(
                    videoManifest.ManifestHash.Hash,
                    videoManifest.Title ?? "",
                    videoManifest.Video.Id,
                    paginatedVideos.Elements.First(pv => pv.Id == videoManifest.Id).Count));
        }

        public async Task<IActionResult> OnPostAsync(int? p)
        {
            var manifest = await indexDbContext.VideoManifests.TryFindOneAsync(vm => vm.ManifestHash.Hash == Input.ManifestHash);

            if (manifest is null)
            {
                ModelState.AddModelError(string.Empty, "Can't find any report for hash");
                await InitializeAsync(p);
                return Page();
            }

            return RedirectToPage("Manage", new { manifestHash = manifest.ManifestHash.Hash, videoId = manifest.Video.Id });
        }

        private IMongoQueryable<VideoReport> VideoReportsWhere(IMongoQueryable<VideoReport> querable)
        {
            if (!string.IsNullOrWhiteSpace(Input?.ManifestHash))
                querable = querable.Where(vr => vr.VideoManifest.ManifestHash.Hash == Input.ManifestHash);
            if (Input is null || 
                !Input.IncludeReportReviewed)
                return querable.Where(vr => vr.VideoManifest.ReviewApproved == null);

            return querable;
        }

    }
}
