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
using System;
using System.Collections.Generic;
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
            if (video.LastValidManifest is null)
            {
                var ex = new InvalidOperationException("Null valid manifest");
                ex.Data.Add("VideoId", video.Id);
                throw ex;
            }

            Id = video.Id;
            CreationDateTime = video.LastValidManifest.CreationDateTime;
            BatchId = video.LastValidManifest.BatchId;
            Description = video.LastValidManifest.Description ?? "";
            Duration = video.LastValidManifest.Duration;
            IsFrozen = video.IsFrozen;
            ManifestHash = video.LastValidManifest.Manifest.Hash;
            OwnerSharedInfoId = video.Owner.SharedInfoId;
            PersonalData = video.LastValidManifest.PersonalData;
            Sources = video.LastValidManifest.Sources.Select(i => new SourceVideoDocument(i.Quality, i.Path, i.Size, i.Type ?? ""));
            Title = video.LastValidManifest.Title ?? "";

            if (video.LastValidManifest.Thumbnail is not null)
                Thumbnail = new ImageDocument(
                    video.LastValidManifest.Thumbnail.AspectRatio,
                    video.LastValidManifest.Thumbnail.Blurhash,
                    video.LastValidManifest.Thumbnail.SourcesV2.Select(s => new SourceImageDocument(s.Width, s.Path, s.Type)));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public VideoDocument() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Id { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string? BatchId { get; set; }
        public string Description { get; set; }
        public long? Duration { get; set; }
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
