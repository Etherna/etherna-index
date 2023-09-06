using System;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoPreviewDto
    {
        // Constructors.
        public VideoPreviewDto(
            Video video,
            UserSharedInfo ownerSharedInfo)
        {
            if (ownerSharedInfo is null)
                throw new ArgumentNullException(nameof(ownerSharedInfo));
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            Id = video.Id;
            if (video.LastValidManifest is not null)
            {
                var manifestDto = new VideoManifest2Dto(video.LastValidManifest);
                CreatedAt = manifestDto.CreatedAt;
                Duration = manifestDto.Duration;
                Thumbnail = manifestDto.Thumbnail;
                Title = manifestDto.Title ?? "";
                UpdatedAt = manifestDto.UpdatedAt;
            }
            OwnerAddress = ownerSharedInfo.EtherAddress;
        }

        // Properties.
        public string Id { get; }
        public long? CreatedAt { get; }
        public long? Duration { get; }
        public string OwnerAddress { get; }
        public Image2Dto? Thumbnail { get; }
        public string? Title { get; }
        public long? UpdatedAt { get; }
    }
}
