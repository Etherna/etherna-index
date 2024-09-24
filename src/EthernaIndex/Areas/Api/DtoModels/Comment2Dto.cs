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
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class Comment2Dto
    {
        public Comment2Dto(Comment comment, UserSharedInfo userSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(comment, nameof(comment));
            ArgumentNullException.ThrowIfNull(userSharedInfo, nameof(userSharedInfo));

            Id = comment.Id;
            CreationDateTime = comment.CreationDateTime;
            IsEditable = comment.IsEditable;
            IsFrozen = comment.IsFrozen;
            OwnerAddress = userSharedInfo.EtherAddress;
            TextHistory = comment.TextHistory;
            VideoId = comment.Video.Id;
        }

        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public bool IsEditable { get; }
        public bool IsFrozen { get; }
        public string OwnerAddress { get; }
        public IReadOnlyDictionary<DateTime, string> TextHistory { get; }
        public string VideoId { get; }
    }
}
