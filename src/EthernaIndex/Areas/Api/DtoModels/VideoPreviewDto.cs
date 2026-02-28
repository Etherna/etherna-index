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

using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoPreviewDto
    {
        // Constructors.
        public VideoPreviewDto(
            Video video,
            UserSharedInfo ownerSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(ownerSharedInfo);
            ArgumentNullException.ThrowIfNull(video);

            Id = video.Id;
            if (video.LastValidManifest is not null)
            {
                var manifestDto = new VideoManifest2Dto(video.LastValidManifest);
                CreatedAt = manifestDto.CreatedAt;
                Duration = manifestDto.Duration;
                Hash = manifestDto.Hash;
                Thumbnail = manifestDto.Thumbnail;
                Title = manifestDto.Title ?? "";
                UpdatedAt = manifestDto.UpdatedAt;
            }
            OwnerAddress = ownerSharedInfo.EtherAddress;
        }

        public VideoPreviewDto(
            VideoDocument videoDocument,
            UserSharedInfo ownerSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(videoDocument);
            ArgumentNullException.ThrowIfNull(ownerSharedInfo);

            Id = videoDocument.Id;
            Duration = videoDocument.Duration;
            Hash = videoDocument.ManifestReference;
            OwnerAddress = ownerSharedInfo.EtherAddress;
            Thumbnail = new Image2Dto(
                videoDocument.Thumbnail.AspectRatio,
                videoDocument.Thumbnail.Blurhash,
                videoDocument.Thumbnail.Sources.Select(s => new ImageSourceDto(s.Type, s.Path, s.Width)));
            Title = videoDocument.Title;
        }

        // Properties.
        public string Id { get; }
        public long? CreatedAt { get; }
        public long? Duration { get; }
        public SwarmReference? Hash { get; }
        public EthAddress OwnerAddress { get; }
        public Image2Dto? Thumbnail { get; }
        public string? Title { get; }
        public long? UpdatedAt { get; }
    }
}
