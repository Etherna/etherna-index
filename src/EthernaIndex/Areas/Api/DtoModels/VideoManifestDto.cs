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
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    [Obsolete("Used only for API backwards compatibility")]
    public class VideoManifestDto
    {
        // Constructors.
        public VideoManifestDto(
            VideoManifest videoManifest)
        {
            ArgumentNullException.ThrowIfNull(videoManifest, nameof(videoManifest));

            Hash = videoManifest.ManifestHash;

            switch (videoManifest.Metadata)
            {
                case null:
                    Sources = Array.Empty<SourceDto>();
                    break;

                case VideoManifestMetadataV1 metadataV1:
                    BatchId = metadataV1.BatchId;
                    Description = metadataV1.Description;
                    Duration = metadataV1.Duration;
                    OriginalQuality = null;
                    PersonalData = metadataV1.PersonalData;
                    Sources = metadataV1.Sources
                        .Select(s => new SourceDto(
                            s.Bitrate,
                            s.Quality,
                            s.Reference,
                            s.Size ?? 0));

                    if (metadataV1.Thumbnail is not null)
                        Thumbnail = new ImageDto(
                            metadataV1.Thumbnail.AspectRatio,
                            metadataV1.Thumbnail.Blurhash,
                            metadataV1.Thumbnail.Sources.ToDictionary(
                                s => s.Key,
                                s => (SwarmAddress)s.Value));

                    Title = metadataV1.Title;
                    break;

                case VideoManifestMetadataV2 metadataV2:
                    BatchId = metadataV2.BatchId;
                    Description = metadataV2.Description;
                    Duration = metadataV2.Duration;
                    PersonalData = metadataV2.PersonalData;
                    Sources = metadataV2.Sources
                        .Select(s => new SourceDto(
                            null,
                            s.Quality ?? "",
                            s.Path.ToSwarmAddress(videoManifest.ManifestHash),
                            s.Size));

                    if (metadataV2.Thumbnail is not null)
                        Thumbnail = new ImageDto(
                            metadataV2.Thumbnail.AspectRatio,
                            metadataV2.Thumbnail.Blurhash,
                            metadataV2.Thumbnail.Sources.ToDictionary(
                                s => $"{s.Width}w",
                                s => s.Path.ToSwarmAddress(videoManifest.ManifestHash)));

                    Title = metadataV2.Title;
                    break;

                default: throw new InvalidOperationException();
            }

        }

        public VideoManifestDto(
            VideoDocument videoDocument)
        {
            ArgumentNullException.ThrowIfNull(videoDocument, nameof(videoDocument));

            BatchId = null;
            Description = videoDocument.Description;
            Duration = videoDocument.Duration;
            Hash = videoDocument.ManifestHash;
            PersonalData = null;
            OriginalQuality = null;
            Sources = [];

            if (videoDocument.Thumbnail is not null)
                Thumbnail = new ImageDto(
                    videoDocument.Thumbnail.AspectRatio,
                    videoDocument.Thumbnail.Blurhash,
                    videoDocument.Thumbnail.Sources.ToDictionary(
                        s => $"{s.Width}w",
                        s => SwarmAddress.FromString(s.Path)));

            Title = videoDocument.Title;
        }

        // Properties.
        public PostageBatchId? BatchId { get; }
        public string? Description { get; }
        public long? Duration { get; }
        public SwarmHash Hash { get; }
        public string? OriginalQuality { get; }
        public string? PersonalData { get; }
        public IEnumerable<SourceDto> Sources { get; }
        public ImageDto? Thumbnail { get; }
        public string? Title { get; }
    }
}
