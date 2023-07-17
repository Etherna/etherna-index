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
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class Video2Dto
    {
        // Constructors.
        public Video2Dto(
            Video video,
            VideoManifest? videoManifest,
            UserSharedInfo ownerSharedInfo,
            VideoVote? currentUserVideoVote)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            if (ownerSharedInfo is null)
                throw new ArgumentNullException(nameof(ownerSharedInfo));

            Id = video.Id;
            CreationDateTime = video.CreationDateTime;
            if (currentUserVideoVote is not null &&
                currentUserVideoVote.Value != VoteValue.Neutral)
            {
                CurrentVoteValue = currentUserVideoVote.Value;
            }
            
            if (videoManifest is not null)
                LastValidManifest = new VideoManifest2Dto(videoManifest);
            OwnerAddress = ownerSharedInfo.EtherAddress;
            TotDownvotes = video.TotDownvotes;
            TotUpvotes = video.TotUpvotes;
        }

        public Video2Dto(
            VideoDocument videoDocument,
            UserSharedInfo ownerSharedInfo,
            VideoVote? currentUserVideoVote)
        {
            if (videoDocument is null)
                throw new ArgumentNullException(nameof(videoDocument));
            if (ownerSharedInfo is null)
                throw new ArgumentNullException(nameof(ownerSharedInfo));

            Id = videoDocument.Id;
            CreationDateTime = videoDocument.CreationDateTime;
            if (currentUserVideoVote is not null &&
                currentUserVideoVote.Value != VoteValue.Neutral)
            {
                CurrentVoteValue = currentUserVideoVote.Value;
            }

            LastValidManifest = new VideoManifest2Dto(videoDocument);
            OwnerAddress = ownerSharedInfo.EtherAddress;
            TotDownvotes = videoDocument.TotDownvotes;
            TotUpvotes = videoDocument.TotUpvotes;
        }

        // Properties.
        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public VoteValue? CurrentVoteValue { get; }
        public VideoManifest2Dto? LastValidManifest { get; }
        public string OwnerAddress { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
