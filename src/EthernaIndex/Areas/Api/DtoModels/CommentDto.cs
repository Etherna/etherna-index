using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class CommentDto
    {
        public CommentDto(Comment comment)
        {
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));

            CreationDateTime = comment.CreationDateTime;
            OwnerAddress = comment.Owner.Address;
            OwnerIdentityManifest = comment.Owner.IdentityManifest?.Hash;
            Text = comment.Text;
            VideoManifestHash = comment.Video.ManifestHash.Hash;
        }

        public DateTime CreationDateTime { get; }
        public string OwnerAddress { get; }
        public string? OwnerIdentityManifest { get; }
        public string Text { get; }
        public string VideoManifestHash { get; }
    }
}
