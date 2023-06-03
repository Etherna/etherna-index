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
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoManifestDto
    {
        // Constructors.
        public VideoManifestDto(
            VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            BatchId = videoManifest.BatchId;
            Description = videoManifest.Description;
            Duration = videoManifest.Duration;
            Hash = videoManifest.Manifest.Hash;
            PersonalData = videoManifest.PersonalData;
            Sources = videoManifest.Sources
                .Select(i => new SourceDto(
                    i.Quality,
                    i.Path,
                    i.Reference,
                    i.Size));

            if (videoManifest.Thumbnail is not null)
                Thumbnail = new ImageDto(
                    videoManifest.Thumbnail.AspectRatio,
                    videoManifest.Thumbnail.Blurhash,
                    videoManifest.Thumbnail.SourcesV2.ToDictionary(s => s.Path ?? "", s => s.Reference ?? ""));

            Title = videoManifest.Title;
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
                    i.Quality,
                    i.Path,
                    i.Reference,
                    i.Size));

            if (videoDocument.Thumbnail is not null)
                Thumbnail = new ImageDto(
                    videoDocument.Thumbnail.AspectRatio,
                    videoDocument.Thumbnail.Blurhash,
                    videoDocument.Thumbnail.Sources.ToDictionary(s => s.Path ?? "", s => s.Reference ?? ""));

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
