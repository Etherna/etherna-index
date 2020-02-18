using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.ApiApplication.V1.DtoModels
{
    public class VideoDto
    {
        // Constructors.
        public VideoDto(Video video)
        {
            Channel = new ChannelDto(video.OwnerChannel);
            CreationDateTime = video.CreationDateTime;
            Description = video.Description;
            ThumbnailHash = video.ThumbnailHash;
            Title = video.Title;
            VideoHash = video.VideoHash;
        }

        // Properties.
        public ChannelDto Channel { get; }
        public DateTime CreationDateTime { get; }
        public string Description { get; }
        public string ThumbnailHash { get; }
        public string Title { get; }
        public string VideoHash { get; }
    }
}
