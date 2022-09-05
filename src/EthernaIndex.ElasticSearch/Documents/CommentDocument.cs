using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using System;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class CommentDocument
    {
        public CommentDocument(Comment comment, UserSharedInfo userSharedInfo)
        {
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));
            if (userSharedInfo is null)
                throw new ArgumentNullException(nameof(userSharedInfo));

            Id = comment.Id;
            CreationDateTime = comment.CreationDateTime;
            IsFrozen = comment.IsFrozen;
            LastUpdateDateTime = comment.LastUpdateDateTime;
            OwnerAddress = userSharedInfo.EtherAddress;
            Text = comment.Text;
            VideoId = comment.Video.Id;
        }

        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public bool IsFrozen { get; }
        public DateTime LastUpdateDateTime { get; }
        public string OwnerAddress { get; }
        public string Text { get; }
        public string VideoId { get; }
    }
}
