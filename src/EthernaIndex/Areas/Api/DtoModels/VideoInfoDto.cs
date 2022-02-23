using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoInfoDto
    {
        // Constructors.
        public VideoInfoDto(
            Video video,
            string? title,
            UserSharedInfo userSharedInfo)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            if (userSharedInfo is null)
                throw new ArgumentNullException(nameof(userSharedInfo));

            Id = video.Id;
            CreationDateTime = video.CreationDateTime;
            EncryptionKey = video.EncryptionKey;
            EncryptionType = video.EncryptionType;
            OwnerAddress = userSharedInfo.EtherAddress;
            Title = title;
            TotDownvotes = video.TotDownvotes;
            TotUpvotes = video.TotUpvotes;
        }

        // Properties.
        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public string? EncryptionKey { get; }
        public EncryptionType EncryptionType { get; }
        public string? Title { get; }
        public string OwnerAddress { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
