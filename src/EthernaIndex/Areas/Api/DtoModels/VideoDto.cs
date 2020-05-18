using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoDto
    {
        // Constructors.
        public VideoDto(Video video)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            ChannelAddress = video.OwnerChannel.Address;
            CreationDateTime = video.CreationDateTime;
            Description = video.Description;
            LengthInSeconds = (int)video.Length.TotalSeconds;
            ThumbnailHash = video.ThumbnailHash;
            Title = video.Title;
            VideoHash = video.VideoHash;
        }

        // Properties.
        public string ChannelAddress { get; }
        public DateTime CreationDateTime { get; }
        public string Description { get; }
        public int LengthInSeconds { get; }
        public string? ThumbnailHash { get; }
        public string Title { get; }
        public string VideoHash { get; }
    }
}
