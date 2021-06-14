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
        public string ManifestHash { get; }
        public string OwnerAddress { get; }
        public string? OwnerIdentityManifest { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
