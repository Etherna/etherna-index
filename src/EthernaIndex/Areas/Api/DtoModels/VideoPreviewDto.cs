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

using System;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoPreviewDto
    {
        // Constructors.
        public VideoPreviewDto(
            Video video,
            UserSharedInfo ownerSharedInfo)
        {
            if (ownerSharedInfo is null)
                throw new ArgumentNullException(nameof(ownerSharedInfo));
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            Id = video.Id;
            if (video.LastValidManifest is not null)
            {
                var manifestDto = new VideoManifest2Dto(video.LastValidManifest);
                CreatedAt = manifestDto.CreatedAt;
                Duration = manifestDto.Duration;
                Thumbnail = manifestDto.Thumbnail;
                Title = manifestDto.Title ?? "";
                UpdatedAt = manifestDto.UpdatedAt;
            }
            OwnerAddress = ownerSharedInfo.EtherAddress;
        }

        // Properties.
        public string Id { get; }
        public long? CreatedAt { get; }
        public long? Duration { get; }
        public string OwnerAddress { get; }
        public Image2Dto? Thumbnail { get; }
        public string? Title { get; }
        public long? UpdatedAt { get; }
    }
}
