// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using System;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class CommentDocument
    {
        // Constructors.
        public CommentDocument(Comment comment, UserSharedInfo userSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(comment);
            ArgumentNullException.ThrowIfNull(userSharedInfo);

            Id = comment.Id;
            CreationDateTime = comment.CreationDateTime;
            IndexingDateTime = DateTime.UtcNow;
            IsFrozen = comment.IsFrozen;
            LastUpdateDateTime = comment.LastUpdateDateTime;
            OwnerAddress = userSharedInfo.EtherAddress.ToString();
            Text = comment.LastText;
            VideoId = comment.Video.Id;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CommentDocument() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Id { get; set; }
        public DateTime CreationDateTime { get; set; }

        /// <summary>
        /// Instant when this document was last (re)indexed. Used by the full reindex task to
        /// detect and remove orphan documents left behind by deletions in the primary store.
        /// </summary>
        public DateTime IndexingDateTime { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public string OwnerAddress { get; set; }
        public string Text { get; set; }
        public string VideoId { get; set; }
    }
}
