//   Copyright 2021-present Etherna Sagl
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));
            if (userSharedInfo is null)
                throw new ArgumentNullException(nameof(userSharedInfo));

            Id = comment.Id;
            CreationDateTime = comment.CreationDateTime;
            IsFrozen = comment.IsFrozen;
            LastUpdateDateTime = comment.LastUpdateDateTime;
            OwnerAddress = userSharedInfo.EtherAddress;
            Text = comment.LastText;
            VideoId = comment.Video.Id;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public CommentDocument() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Id { get; set; }
        public DateTime CreationDateTime { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime LastUpdateDateTime { get; set; }
        public string OwnerAddress { get; set; }
        public string Text { get; set; }
        public string VideoId { get; set; }
    }
}
