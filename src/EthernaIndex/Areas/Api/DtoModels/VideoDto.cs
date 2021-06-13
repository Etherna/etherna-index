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

            CreationDateTime = video.CreationDateTime;
            EncryptionKey = video.EncryptionKey;
            EncryptionType = video.EncryptionType;
            FairDrivePath = video.FairDrivePath;
            ManifestHash = video.ManifestHash.Hash;
            OwnerAddress = video.Owner.Address;
            OwnerIdentityManifest = video.Owner.IdentityManifest?.Hash;
            TotDownvotes = video.TotDownvotes;
            TotUpvotes = video.TotUpvotes;
        }

        // Properties.
        public DateTime CreationDateTime { get; }
        public string? EncryptionKey { get; }
        public EncryptionType EncryptionType { get; }
        public string? FairDrivePath { get; }
        public string ManifestHash { get; }
        public string OwnerAddress { get; }
        public string? OwnerIdentityManifest { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
