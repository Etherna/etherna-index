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

using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoManifest2Dto
    {
        // Constructors.
        public VideoManifest2Dto(
            VideoManifest videoManifest)
        {
            ArgumentNullException.ThrowIfNull(videoManifest, nameof(videoManifest));

            Hash = videoManifest.Manifest.Hash;

            switch (videoManifest.Metadata)
            {
                case null:
                    Sources = Array.Empty<VideoSourceDto>();
                    break;

                case VideoManifestMetadataV1 metadataV1:
                    BatchId = metadataV1.BatchId;
                    CreatedAt = metadataV1.CreatedAt ?? 0;
                    Description = metadataV1.Description;
                    Duration = metadataV1.Duration;
                    PersonalData = metadataV1.PersonalData;
                    Sources = metadataV1.Sources
                        .Select(s => new VideoSourceDto(
                            "mp4",
                            s.Quality,
                            s.Reference,
                            s.Size ?? 0));

                    if (metadataV1.Thumbnail is not null)
                        Thumbnail = new Image2Dto(
                            metadataV1.Thumbnail.AspectRatio,
                            metadataV1.Thumbnail.Blurhash,
                            metadataV1.Thumbnail.Sources.Select(s => new ImageSourceDto(
                                null,
                                s.Value,
                                int.Parse(s.Key.Replace("w", "", StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture))));

                    Title = metadataV1.Title;
                    UpdatedAt = metadataV1.UpdatedAt;
                    break;

                case VideoManifestMetadataV2 metadataV2:
                    AspectRatio = metadataV2.AspectRatio;
                    BatchId = metadataV2.BatchId;
                    CreatedAt = metadataV2.CreatedAt;
                    Description = metadataV2.Description;
                    Duration = metadataV2.Duration;
                    PersonalData = metadataV2.PersonalData;
                    Sources = metadataV2.Sources
                        .Select(s => new VideoSourceDto(
                            s.Type,
                            s.Quality,
                            s.Path,
                            s.Size));

                    if (metadataV2.Thumbnail is not null)
                        Thumbnail = new Image2Dto(
                            metadataV2.Thumbnail.AspectRatio,
                            metadataV2.Thumbnail.Blurhash,
                            metadataV2.Thumbnail.Sources.Select(s => new ImageSourceDto(s.Type, s.Path, s.Width)));

                    Title = metadataV2.Title;
                    UpdatedAt = metadataV2.UpdatedAt;
                    break;

                default: throw new InvalidOperationException();
            }
        }

        // Properties.
        public float AspectRatio { get; }
        public string? BatchId { get; }
        public long CreatedAt { get; }
        public string? Description { get; }
        public long? Duration { get; }
        public string Hash { get; }
        public string? PersonalData { get; }
        public IEnumerable<VideoSourceDto> Sources { get; }
        public Image2Dto? Thumbnail { get; }
        public string? Title { get; }
        public long? UpdatedAt { get; }
    }
}
