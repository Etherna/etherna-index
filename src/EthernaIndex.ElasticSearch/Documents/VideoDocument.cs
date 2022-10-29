﻿//   Copyright 2021-present Etherna Sagl
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
            OriginalQuality = video.LastValidManifest.OriginalQuality;
            OwnerSharedInfoId = video.Owner.SharedInfoId;
            Sources = video.LastValidManifest.Sources.Select(i => new SourceDocument(i.Bitrate, i.Quality, i.Reference, i.Size));
            Title = video.LastValidManifest.Title ?? "";

            if (video.LastValidManifest.Thumbnail is not null)
                Thumbnail = new ImageDocument(
                    video.LastValidManifest.Thumbnail.AspectRatio,
                    video.LastValidManifest.Thumbnail.Blurhash,
                    video.LastValidManifest.Thumbnail.Sources);
        }

        // Properties.
        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public string? BatchId { get; }
        public string Description { get; }
        public long? Duration { get; }
        public bool IsFrozen { get; }
        public string ManifestHash { get; }
        public string? OriginalQuality { get; }
        public string OwnerSharedInfoId { get; }
        public IEnumerable<SourceDocument> Sources { get; }
        public ImageDocument? Thumbnail { get; }
        public string Title { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}