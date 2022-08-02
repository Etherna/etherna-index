using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoManifestDto
    {
        // Constructors.
        public VideoManifestDto(
            VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            Hash = videoManifest.Manifest.Hash;
            Description = videoManifest.Description;
            Duration = videoManifest.Duration;
            OriginalQuality = videoManifest.OriginalQuality;
            Sources = videoManifest.Sources?
                .Select(i => new SourceDto(
                    i.Bitrate,
                    i.Quality,
                    i.Reference,
                    i.Size));
            Title = videoManifest.Title;

            if (videoManifest.Thumbnail is not null)
                Thumbnail = new ImageDto(
                    videoManifest.Thumbnail.AspectRatio,
                    videoManifest.Thumbnail.Blurhash,
                    videoManifest.Thumbnail.Sources);
        }

        // Properties.
        public string Hash { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? OriginalQuality { get; set; }
        public float? Duration { get; set; }
        public ImageDto? Thumbnail { get; set; }
        public IEnumerable<SourceDto>? Sources { get; set; }
    }
}
