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
            EncryptionKey = video.EncryptionKey;
            EncryptionType = video.EncryptionType;
            LengthInSeconds = (int)video.Length.TotalSeconds;
            ThumbnailHash = video.ThumbHash?.Hash;
            ThumbnailHashIsRaw = video.ThumbHash?.IsRaw ?? false;
            Title = video.Title;
            VideoHash = video.Hash.Hash;
            VideoHashIsRaw = video.Hash.IsRaw;
        }

        // Properties.
        public string ChannelAddress { get; }
        public DateTime CreationDateTime { get; }
        public string Description { get; }
        public string? EncryptionKey { get; }
        public EncryptionType EncryptionType { get; }
        public int LengthInSeconds { get; }
        public string? ThumbnailHash { get; }
        public bool ThumbnailHashIsRaw { get; }
        public string Title { get; }
        public string VideoHash { get; }
        public bool VideoHashIsRaw { get; }
    }
}
