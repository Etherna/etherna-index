using Etherna.Authentication;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.Videos
{
    public class IndexModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Display(Name = "Reason")]
            public string? Reason { get; set; }
        }

        public class ManifestModel
        {
            // Constructors.
            public ManifestModel(VideoManifest videoManifest)
            {
                if (videoManifest == null)
                    throw new ArgumentNullException(nameof(videoManifest));

                Id = videoManifest.Id;
                CreationDateTime = videoManifest.CreationDateTime;
                IsValid = videoManifest.IsValid;
                ManifestHash = videoManifest.Manifest.Hash;
                OwnerAddress = videoManifest.Id;
                ValidationTime = videoManifest.ValidationTime;

                switch (videoManifest.Metadata)
                {
                    case null:
                        Title = videoManifest.Id;
                        break;

                    case VideoManifestMetadataV1 metadataV1:
                        Description = metadataV1.Description;
                        Duration = metadataV1.Duration;
                        Title = metadataV1.Title;
                        break;

                    case VideoManifestMetadataV2 metadataV2:

                        Description = metadataV2.Description;
                        Duration = metadataV2.Duration;
                        Title = metadataV2.Title;
                        break;

                    default: throw new InvalidOperationException();
                }
            }

            // Properties.
            public string Id { get; set; } = default!;
            public DateTime CreationDateTime { get; set; }
            public string? Description { get; set; } = default!;
            public float? Duration { get; set; }
            public IEnumerable<string> ErrorsDetails { get; set; } = default!;
            public bool? IsValid { get; set; }
            public string ManifestHash { get; set; } = default!;
            public string OwnerAddress { get; set; } = default!;
            public string? Title { get; set; } = default!;
            public DateTime? ValidationTime { get; set; }
        }

        public class VideoModel
        {
            // Constructors.
            public VideoModel(
                Video video, 
                VideoManifest? videoManifest,
                IEnumerable<VideoReportModel> detailReports)
            {
                if (video is null)
                    throw new ArgumentNullException(nameof(video));

                VideoId = video.Id;
                Reports = detailReports;

                if (videoManifest is not null)
                    Manifest = new ManifestModel(videoManifest);
            }

            // Properties.
            public string VideoId { get; set; }
            public ManifestModel Manifest { get; private set; } = default!;
            public IEnumerable<VideoReportModel> Reports { get; private set; } = default!;
        }

        public class VideoReportModel
        {
            // Constructors.
            public VideoReportModel(
                string id,
                string reportAddress,
                string reportDescription,
                DateTime reportDate)
            {
                if (id is null)
                    throw new ArgumentNullException(nameof(id));

                Id = id;
                Address = reportAddress;
                Date = reportDate;
                Description = reportDescription;
                DetailType = UnsuitableReportDetailType.ReportVideo;
            }

            public VideoReportModel(
                string id,
                string reviewerAddress,
                string reviewDescription,
                DateTime reviewDate,
                bool isValid)
            {
                if (id is null)
                    throw new ArgumentNullException(nameof(id));

                Id = id;
                Address = reviewerAddress;
                Date = reviewDate;
                Description = reviewDescription;
                DetailType = UnsuitableReportDetailType.ManualVideoReview;
                IsValid = isValid;
            }

            // Properties.
            public string Id { get; }
            public string Address { get; }
            public string Description { get; }
            public DateTime Date { get; }
            public UnsuitableReportDetailType DetailType { get; }
            public bool IsValid { get; private set; }

            // Enums.
            public enum UnsuitableReportDetailType
            {
                ManualVideoReview,
                ReportVideo
            }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly IUserService userService;

        // Constructor.
        public IndexModel(
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IIndexDbContext indexDbContext,
            IUserService userService)
        {
            if (indexDbContext is null)
                throw new ArgumentNullException(nameof(indexDbContext));

            this.CustomRouteData = new Dictionary<string, string>();
            this.ethernaOidcClient = ethernaOidcClient;
            this.indexDbContext = indexDbContext;
            this.userService = userService;
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;
        public int CurrentPage { get; private set; }
        public Dictionary<string, string> CustomRouteData { get; private set; }
        public long MaxPage { get; private set; }
        public VideoModel VideoModelSelected { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(
            string videoId, 
            int? p)
        {
            ManageCustomRouteData(videoId, p);

            // Video info
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == videoId);

            // Unsuitable video reports.
            var paginatedUnsuitableVideoReports = await indexDbContext.UnsuitableVideoReports.QueryPaginatedElementsAsync(
                vm => vm.Where(i => i.Video.Id == video.Id),
                vm => vm.CreationDateTime,
                CurrentPage,
                PageSize);
            var videoReportDetails = paginatedUnsuitableVideoReports.Elements
                .Select(e => new VideoReportModel(
                        e.Id,
                        e.ReporterAuthor.SharedInfoId,
                        e.Description,
                        e.CreationDateTime));
            MaxPage = paginatedUnsuitableVideoReports.MaxPage;

            // Manual video reviews.
            var paginatedManualVideoReviews = await indexDbContext.ManualVideoReviews.QueryPaginatedElementsAsync(
                vr => vr.Where(i => i.Video.Id == videoId),
                vr => vr.CreationDateTime,
                CurrentPage,
                PageSize);
            MaxPage += paginatedManualVideoReviews.MaxPage;
            videoReportDetails = videoReportDetails.Union(paginatedManualVideoReviews.Elements
                .Select(e => new VideoReportModel(
                        e.Id,
                        e.Author.SharedInfoId,
                        e.Description,
                        e.CreationDateTime,
                        e.IsValidResult)));

            VideoModelSelected = new VideoModel(
                video, 
                video.LastValidManifest,
                videoReportDetails.Take(PageSize).OrderByDescending(i => i.Date));
        }

        public async Task<IActionResult> OnPostApproveVideo(
            string videoId, 
            int? p)
        {
            ManageCustomRouteData(videoId, p);

            await CreateReviewAsync(videoId, true, Input.Reason ?? "");

            return RedirectToPage("Index", CustomRouteData);
        }

        public async Task<IActionResult> OnPostRejectVideo(
            string videoId, 
            int? p)
        {
            ManageCustomRouteData(videoId, p);

            await CreateReviewAsync(videoId, false, Input.Reason ?? "");

            return RedirectToPage("Index", CustomRouteData);
        }

        // Helpers.
        private async Task CreateReviewAsync(string videoId, bool isValid, string motivation)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, _) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            // Create ManualReview.
            await indexDbContext.ManualVideoReviews.CreateAsync(new ManualVideoReview(user, motivation, isValid, video));
        }

        private void ManageCustomRouteData(
            string videoId,
            int? p)
        {
            if (CustomRouteData.ContainsKey("videoId"))
                CustomRouteData["videoId"] = videoId;
            else
                CustomRouteData.Add("videoId", videoId);
            CurrentPage = p ?? 0;
        }

    }
}
