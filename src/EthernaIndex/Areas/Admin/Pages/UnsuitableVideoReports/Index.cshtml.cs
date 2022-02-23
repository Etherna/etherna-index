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

namespace Etherna.EthernaIndex.Areas.Admin.Pages.UnsuitableVideoReports
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
            string manifestHash,
            string videoId,
            int? p)
        {
            await InitializeAsync(
                manifestHash,
                videoId,
                p);
        }

        public async Task OnPost()
        {
            await InitializeAsync(
                Input?.ManifestHash,
                Input?.VideoId,
                null);
        }

        // Helpers.
        private async Task InitializeAsync(
            string? manifestHash,
            string? videoId,
            int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedUnsuitableReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                vm => VideoUnsuitableReportWhere(vm, manifestHash, videoId)
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
                vm.Manifest.Hash,
                vm.Title ?? "",
                vm.Video.Id,
                paginatedUnsuitableReports.Elements.First(pv => pv.Id == vm.Id).Count));
        }

        private IMongoQueryable<UnsuitableVideoReport> VideoUnsuitableReportWhere(
            IMongoQueryable<UnsuitableVideoReport> querable,
            string? manifestHash,
            string? videoId)
        {
            if (!string.IsNullOrWhiteSpace(manifestHash))
                querable = querable.Where(vur => vur.VideoManifest.Manifest.Hash == manifestHash);
            if (!string.IsNullOrWhiteSpace(videoId))
                querable = querable.Where(vur => vur.VideoManifest.Video.Id == videoId);

            return querable;
        }
    }
}
