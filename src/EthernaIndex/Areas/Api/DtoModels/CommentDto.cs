﻿//   Copyright 2021-present Etherna Sagl
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
            VideoId = comment.Video.Id;
        }

        public DateTime CreationDateTime { get; }
        public string OwnerAddress { get; }
        public string? OwnerIdentityManifest { get; }
        public string Text { get; }
        public string VideoId { get; }
    }
}
