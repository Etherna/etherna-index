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
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            Hash = videoManifest.Manifest.Hash;

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
                            metadataV1.Thumbnail.Sources);

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
                            s.Path,
                            s.Size));

                    if (metadataV2.Thumbnail is not null)
                        Thumbnail = new ImageDto(
                            metadataV2.Thumbnail.AspectRatio,
                            metadataV2.Thumbnail.Blurhash,
                            metadataV2.Thumbnail.Sources.ToDictionary(s => $"{s.Width}w", s => s.Path));

                    Title = metadataV2.Title;
                    break;

                default: throw new InvalidOperationException();
            }

        }

        public VideoManifestDto(
            VideoDocument videoDocument)
        {
            if (videoDocument is null)
                throw new ArgumentNullException(nameof(videoDocument));

            BatchId = videoDocument.BatchId;
            Description = videoDocument.Description;
            Duration = videoDocument.Duration;
            Hash = videoDocument.ManifestHash;
            PersonalData = videoDocument.PersonalData;
            OriginalQuality = videoDocument.OriginalQuality;
            Sources = videoDocument.Sources
                .Select(i => new SourceDto(
                    null,
                    i.Quality,
                    i.Path,
                    i.Size));

            if (videoDocument.Thumbnail is not null)
                Thumbnail = new ImageDto(
                    videoDocument.Thumbnail.AspectRatio,
                    videoDocument.Thumbnail.Blurhash,
                    videoDocument.Thumbnail.Sources.ToDictionary(s => $"{s.Width}w", s => s.Path));

            Title = videoDocument.Title;
        }

        // Properties.
        public string? BatchId { get; }
        public string? Description { get; }
        public long? Duration { get; }
        public string Hash { get; }
        public string? OriginalQuality { get; }
        public string? PersonalData { get; }
        public IEnumerable<SourceDto> Sources { get; }
        public ImageDto? Thumbnail { get; }
        public string? Title { get; }
    }
}
