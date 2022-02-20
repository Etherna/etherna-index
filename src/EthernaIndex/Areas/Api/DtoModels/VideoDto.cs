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

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoDto
    {
        // Constructors.
        public VideoDto(
            Video video,
            VideoManifest? lastValidManifest, 
            UserSharedInfo userSharedInfo)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            if (userSharedInfo is null)
                throw new ArgumentNullException(nameof(userSharedInfo));

            if (lastValidManifest is not null &&
                lastValidManifest.Video.Id != video.Id)
            {
                var ex = new InvalidOperationException("Video not compatible with current Manifest");
                ex.Data.Add("VideoId", video.Id);
                ex.Data.Add("ManifestHash", lastValidManifest.ManifestHash.Hash);
                ex.Data.Add("Manifest.VideoId", lastValidManifest.Video.Id);
                throw ex;
            }

            Id = video.Id;
            CreationDateTime = video.CreationDateTime;
            EncryptionKey = video.EncryptionKey;
            EncryptionType = video.EncryptionType;
            if (lastValidManifest is not null)
                LastValidManifest = new VideoManifestDto(lastValidManifest);
            OwnerAddress = userSharedInfo.EtherAddress;
            OwnerIdentityManifest = video.Owner.IdentityManifest?.Hash;
            TotDownvotes = video.TotDownvotes;
            TotUpvotes = video.TotUpvotes;
        }

        // Properties.
        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public string? EncryptionKey { get; }
        public EncryptionType EncryptionType { get; }
        public VideoManifestDto? LastValidManifest { get; }
        public string OwnerAddress { get; }
        public string? OwnerIdentityManifest { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
