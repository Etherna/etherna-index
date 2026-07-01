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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.SwarmSdk.Models;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class ManifestModel : PageModel
    {
        // Models.
        public class VideoManifestDto
        {
            // Constructor.
            public VideoManifestDto(
                Video? video,
                VideoManifest videoManifest)
            {
                ArgumentNullException.ThrowIfNull(videoManifest);

                Id = videoManifest.Id;
                CreationDateTime = videoManifest.CreationDateTime;
                ErrorsDetails = videoManifest.ValidationErrors.Select(i => $"[{i.ErrorType}]: {i.ErrorMessage}");
                IsValid = videoManifest.IsValid;
                ManifestReference = videoManifest.ManifestReference;
                OwnerAddress = videoManifest.Id;
                VideoInfo = video is null ? null : new VideoInfoDto(video);
                ValidationTime = videoManifest.ValidationTime;

                switch (videoManifest.Metadata)
                {
                    case null:
                        Title = videoManifest.Id;
                        Sources = new List<MetadataVideoSourceDto>();
                        break;

                    case VideoManifestMetadataV1 metadataV1:
                        Description = metadataV1.Description;
                        Duration = metadataV1.Duration;
                        Title = metadataV1.Title;
                        Sources = metadataV1.Sources.Select(s => new MetadataVideoSourceDto(
                            s.Reference,
                            s.Size ?? 0,
                            s.Quality));
                        Thumbnail = metadataV1.Thumbnail != null ?
                            new SwarmImageRawDto(
                                metadataV1.Thumbnail.AspectRatio,
                                metadataV1.Thumbnail.Blurhash,
                                metadataV1.Thumbnail.Sources.ToDictionary(
                                    s => s.Key,
                                    s => (SwarmAddress)s.Value)) :
                            null;
                        break;

                    case VideoManifestMetadataV2 metadataV2:
                        Description = metadataV2.Description;
                        Duration = metadataV2.Duration;
                        Title = metadataV2.Title;
                        Sources = metadataV2.Sources.Select(i => new MetadataVideoSourceDto(
                                i.Path.ToSwarmAddress(videoManifest.ManifestReference),
                                i.Size,
                                i.Quality ?? ""));
                        Thumbnail = metadataV2.Thumbnail != null ?
                            new SwarmImageRawDto(
                                metadataV2.Thumbnail.AspectRatio,
                                metadataV2.Thumbnail.Blurhash,
                                metadataV2.Thumbnail.Sources.ToDictionary(
                                    s => s.Width.ToString(CultureInfo.InvariantCulture),
                                    s => s.Path.ToSwarmAddress(videoManifest.ManifestReference))) :
                            null;
                        break;

                    default: throw new InvalidOperationException();
                }
            }

            // Properties.
            public string Id { get; set; }
            public DateTime CreationDateTime { get; set; }
            public string? Description { get; set; }
            public float? Duration { get; set; }
            public IEnumerable<string> ErrorsDetails { get; set; }
            public bool? IsValid { get; set; }
            public SwarmReference ManifestReference { get; set; }
            public string OwnerAddress { get; set; }
            public IEnumerable<MetadataVideoSourceDto> Sources { get; set; }
            public SwarmImageRawDto? Thumbnail { get; set; }
            public string? Title { get; set; }
            public VideoInfoDto? VideoInfo { get; set; }
            public DateTime? ValidationTime { get; set; }
        }

        public class MetadataVideoSourceDto(
            SwarmAddress address,
            long size,
            string quality)
        {
            public SwarmAddress Address { get; set; } = address;
            public long Size { get; set; } = size;
            public string Quality { get; set; } = quality;
        }

        public class SwarmImageRawDto(
            float aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, SwarmAddress> sources)
        {
            public float AspectRatio { get; set; } = aspectRatio;
            public string Blurhash { get; set; } = blurhash;
            public IReadOnlyDictionary<string, SwarmAddress> Sources { get; set; } = sources;
        }

        public class VideoInfoDto(Video video)
        {
            public string VideoId { get; set; } = video.Id;
        }

        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public ManifestModel(
            IBackgroundJobClient backgroundJobClient,
            IIndexDbContext indexDbContext)
        {
            ArgumentNullException.ThrowIfNull(indexDbContext);

            this.backgroundJobClient = backgroundJobClient;
            this.indexDbContext = indexDbContext;
        }

        // Properties.
        public VideoManifestDto VideoManifest { get; private set; } = default!;

        // Methods.
        public async Task OnGetAsync(SwarmReference manifestReference)
        {
            // Video info
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.ManifestReference == manifestReference);
            var video = await indexDbContext.Videos.TryFindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            VideoManifest = new VideoManifestDto(video, videoManifest);
        }

        public async Task<IActionResult> OnPostForceNewValidationAsync(string manifestReference)
        {
            // Get Manifest & Video data.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(c => c.ManifestReference == manifestReference);
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            // Background Validator.
            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, videoManifest.ManifestReference.ToString()),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            return RedirectToPage("Index");
        }
    }
}
