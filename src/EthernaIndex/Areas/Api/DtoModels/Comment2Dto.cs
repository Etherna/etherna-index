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
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class Comment2Dto
    {
        public Comment2Dto(Comment comment, UserSharedInfo userSharedInfo)
        {
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));
            if (userSharedInfo is null)
                throw new ArgumentNullException(nameof(userSharedInfo));

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
