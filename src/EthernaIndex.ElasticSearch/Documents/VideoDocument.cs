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
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class VideoDocument
    {
        // Constructors.
        public VideoDocument(
            Video video)
        { 
            if (video is null)
            {
                throw new ArgumentNullException(nameof(video));
            }
            if (video.LastValidManifest?.Metadata is null)
            {
                var ex = new InvalidOperationException("Null last valid manifest");
                ex.Data.Add("VideoId", video.Id);
                throw ex;
            }

            Id = video.Id;
            CreationDateTime = video.LastValidManifest.CreationDateTime;
            IsFrozen = video.IsFrozen;
            ManifestHash = video.LastValidManifest.Manifest.Hash;
            OwnerSharedInfoId = video.Owner.SharedInfoId;

            switch (video.LastValidManifest.Metadata)
            {
                case VideoManifestMetadataV1 metadataV1:
                    BatchId = metadataV1.BatchId;
                    Description = metadataV1.Description;
                    Duration = metadataV1.Duration;
                    PersonalData = metadataV1.PersonalData;
                    Sources = metadataV1.Sources.Select(i => new SourceVideoDocument(i.Reference, i.Quality, i.Size ?? 0, "mp4"));
                    Title = metadataV1.Title;

                    if (metadataV1.Thumbnail is not null)
                        Thumbnail = new ImageDocument(
                            metadataV1.Thumbnail.AspectRatio,
                            metadataV1.Thumbnail.Blurhash,
                            metadataV1.Thumbnail.Sources.Select(s =>
                                new SourceImageDocument(
                                    int.Parse(s.Key.Replace("w", "", StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture),
                                    s.Value,
                                    null)));
                    break;

                case VideoManifestMetadataV2 metadataV2:
                    BatchId = metadataV2.BatchId;
                    Description = metadataV2.Description;
                    Duration = metadataV2.Duration;
                    PersonalData = metadataV2.PersonalData;
                    Sources = metadataV2.Sources.Select(i => new SourceVideoDocument(i.Path, i.Quality, i.Size, i.Type));
                    Title = metadataV2.Title;

                    if (metadataV2.Thumbnail is not null)
                        Thumbnail = new ImageDocument(
                            metadataV2.Thumbnail.AspectRatio,
                            metadataV2.Thumbnail.Blurhash,
                            metadataV2.Thumbnail.Sources.Select(s => new SourceImageDocument(s.Width, s.Path, s.Type)));
                    break;

                default: throw new InvalidOperationException();
            }
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public VideoDocument() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Id { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string? BatchId { get; set; }
        public string Description { get; set; }
        public long Duration { get; set; }
        public bool IsFrozen { get; set; }
        public string ManifestHash { get; set; }
        public string? OriginalQuality { get; set; }
        public string OwnerSharedInfoId { get; set; }
        public string? PersonalData { get; set; }
        public IEnumerable<SourceVideoDocument> Sources { get; set; }
        public ImageDocument? Thumbnail { get; set; }
        public string Title { get; set; }
        public long TotDownvotes { get; set; }
        public long TotUpvotes { get; set; }
    }
}
