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

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoUnsuitableReports
{
    public class ReportModel : PageModel
    {

        // Models.
        public class InputModel
        {
            [Display(Name = "Include Reviewed")]
            public bool IncludeReviewed { get; set; } = default!;

            [Display(Name = "Manifest Hash")]
            public string? ManifestHash { get; set; }

            [Display(Name = "Video Id")]
            public string? VideoId { get; set; }
        }

        public class VideoUnsuitableReportDto
        {
            public VideoUnsuitableReportDto(
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
        public IEnumerable<VideoUnsuitableReportDto> VideoUnsuitableReports { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(
            bool includeReportReviewed,
            string manifestHash,
            string videoId,
            int? p)
        {
            await InitializeAsync(
                includeReportReviewed,
                manifestHash,
                videoId,
                p);
        }

        // Helpers.
        private async Task InitializeAsync(
            bool includeReportReviewed,
            string? manifestHash,
            string? videoId,
            int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedUnsuitableReports = await indexDbContext.VideoUnsuitableReports.QueryPaginatedElementsAsync(
                vm => VideoUnsuitableReportWhere(vm, includeReportReviewed, manifestHash, videoId)
                        .GroupBy(i => i.VideoManifest.Id)
                        .Select(group => new
                        {
                            Id = group.Key,
                            Count = group.Count()
                        }),
                vm => vm.Id,
                CurrentPage,
                PageSize);

            MaxPage = paginatedUnsuitableReports.MaxPage;

            var videoManifestIds = paginatedUnsuitableReports.Elements.Select(e => e.Id);

            // Get manifest info.
            var videoManifests = await indexDbContext.VideoManifests.QueryElementsAsync(elements =>
               elements.Where(u => videoManifestIds.Contains(u.Id))
                       .OrderBy(i => i.Id)
                       .ToListAsync());

            VideoUnsuitableReports = videoManifests.Select(vm => new VideoUnsuitableReportDto(
                vm.ManifestHash.Hash,
                vm.Title ?? "",
                vm.Video.Id,
                paginatedUnsuitableReports.Elements.First(pv => pv.Id == vm.Id).Count));
        }

        public async Task OnPost()
        {
            await InitializeAsync(
                Input?.IncludeReviewed ?? false,
                Input?.ManifestHash,
                Input?.VideoId,
                null);
        }

        private IMongoQueryable<VideoUnsuitableReport> VideoUnsuitableReportWhere(
            IMongoQueryable<VideoUnsuitableReport> querable,
            bool includeReportReviewed,
            string? manifestHash,
            string? videoId)
        {
            if (!string.IsNullOrWhiteSpace(manifestHash))
                querable = querable.Where(vur => vur.VideoManifest.ManifestHash.Hash == manifestHash);
            if (!string.IsNullOrWhiteSpace(videoId))
                querable = querable.Where(vur => vur.VideoManifest.Video.Id == videoId);
            if (!includeReportReviewed)
                return querable.Where(vur => vur.VideoManifest.ReviewApproved == null);

            return querable;
        }

    }
}
