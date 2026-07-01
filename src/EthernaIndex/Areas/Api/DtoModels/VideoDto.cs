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
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.SwarmSdk.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    [Obsolete("Used only for API backwards compatibility")]
    public class VideoDto
    {
        // Constructors.
        public VideoDto(
            Video video,
            VideoManifest? videoManifest,
            UserSharedInfo ownerSharedInfo,
            VideoVote? currentUserVideoVote)
        {
            ArgumentNullException.ThrowIfNull(video);
            ArgumentNullException.ThrowIfNull(ownerSharedInfo);

            Id = video.Id;
            CreationDateTime = video.CreationDateTime;
            if (currentUserVideoVote is not null &&
                currentUserVideoVote.Value != VoteValue.Neutral)
            {
                CurrentVoteValue = currentUserVideoVote.Value;
            }
            
            if (videoManifest is not null)
                LastValidManifest = new VideoManifestDto(videoManifest);
            OwnerAddress = ownerSharedInfo.EtherAddress;
            TotDownvotes = video.TotDownvotes;
            TotUpvotes = video.TotUpvotes;
        }

        public VideoDto(
            VideoDocument videoDocument,
            UserSharedInfo ownerSharedInfo,
            VideoVote? currentUserVideoVote)
        {
            ArgumentNullException.ThrowIfNull(videoDocument);
            ArgumentNullException.ThrowIfNull(ownerSharedInfo);

            Id = videoDocument.Id;
            CreationDateTime = videoDocument.CreationDateTime;
            if (currentUserVideoVote is not null &&
                currentUserVideoVote.Value != VoteValue.Neutral)
            {
                CurrentVoteValue = currentUserVideoVote.Value;
            }

            LastValidManifest = new VideoManifestDto(videoDocument);
            OwnerAddress = ownerSharedInfo.EtherAddress;
            TotDownvotes = videoDocument.TotDownvotes;
            TotUpvotes = videoDocument.TotUpvotes;
        }

        // Properties.
        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public VoteValue? CurrentVoteValue { get; }
        public VideoManifestDto? LastValidManifest { get; }
        public EthAddress OwnerAddress { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
