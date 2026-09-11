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
using Etherna.SwarmSdk.Models;
using System;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    [Obsolete("Used only for API backwards compatibility")]
    public class CommentDto
    {
        public CommentDto(Comment comment, UserSharedInfo userSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(comment);
            ArgumentNullException.ThrowIfNull(userSharedInfo);

            Id = comment.Id;
            CreationDateTime = comment.CreationDateTime;
            IsFrozen = comment.IsFrozen;
            LastUpdateDateTime = comment.TextHistory.Keys.Max();
            OwnerAddress = userSharedInfo.EtherAddress;
            Text = comment.TextHistory.MaxBy(p => p.Key).Value;
            VideoId = comment.Video.Id;
        }
        public CommentDto(Comment2Dto comment)
        {
            ArgumentNullException.ThrowIfNull(comment);

            Id = comment.Id;
            CreationDateTime = comment.CreationDateTime;
            IsFrozen = comment.IsFrozen;
            LastUpdateDateTime = comment.TextHistory.Keys.Max();
            OwnerAddress = comment.OwnerAddress;
            Text = comment.TextHistory.MaxBy(p => p.Key).Value;
            VideoId = comment.VideoId;
        }

        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public bool IsFrozen { get; }
        public DateTime LastUpdateDateTime { get; }
        public EthAddress OwnerAddress { get; }
        public string Text { get; }
        public string VideoId { get; }
    }
}
