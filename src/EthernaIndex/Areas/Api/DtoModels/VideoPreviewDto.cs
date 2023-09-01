using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoPreviewDto
    {
        // Constructors.
        public VideoPreviewDto(
            Video video,
            VideoManifest videoManifest,
            UserSharedInfo ownerSharedInfo)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            if (ownerSharedInfo is null)
                throw new ArgumentNullException(nameof(ownerSharedInfo));

            Id = video.Id;
            var manifestDto = new VideoManifest2Dto(videoManifest);
            AspectRatio = manifestDto.AspectRatio;
            CreatedAt = manifestDto.CreatedAt;
            Duration = manifestDto.Duration;
            Thumbnail = manifestDto.Thumbnail;
            Title = manifestDto.Title ?? "";
            UpdatedAt = manifestDto.UpdatedAt;
            switch (videoManifest.Metadata)
            {
                case VideoManifestMetadataV1:
                    V = "1.x";
                    break;
                case VideoManifestMetadataV2:
                    V = "2.0";
                    break;
                default: throw new InvalidOperationException("VideoManifestMetadataV not found");
            }
        }

        // Properties.
        public string Id { get; }
        public float AspectRatio { get; }
        public long CreatedAt { get; }
        public long? Duration { get; }
        public Image2Dto? Thumbnail { get; }
        public string? Title { get; }
        public long? UpdatedAt { get; }
        public string? V { get; }
    }
}
