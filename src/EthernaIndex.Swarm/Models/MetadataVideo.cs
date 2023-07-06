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

using Etherna.EthernaIndex.Swarm.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class MetadataVideo
    {
        // Constructors.
        public MetadataVideo(
            float? aspectRatio,
            string? batchId,
            string description,
            long duration,
            long createdAt,
            string ownerAddress,
            string? personalData,
            IEnumerable<MetadataVideoSource> sources,
            SwarmImageRaw? thumbnail,
            string title,
            long? updatedAt,
            Version version)
        {
            AspectRatio = aspectRatio;
            BatchId = batchId;
            Description = description;
            Duration = duration;
            CreatedAt = createdAt;
            OwnerAddress = ownerAddress;
            PersonalData = personalData;
            Sources = sources;
            Thumbnail = thumbnail;
            Title = title;
            UpdatedAt = updatedAt;
            Version = version;
        }

        internal MetadataVideo(MetadataVideoSchema1 metadataVideo) : this(
            null,
            metadataVideo.BatchId,
            metadataVideo.Description,
            metadataVideo.Duration,
            metadataVideo.CreatedAt,
            metadataVideo.OwnerAddress,
            metadataVideo.PersonalData,
            metadataVideo.Sources.Select(s => new MetadataVideoSource(s)),
            metadataVideo.Thumbnail is null ? null : new SwarmImageRaw(metadataVideo.Thumbnail),
            metadataVideo.Title,
            metadataVideo.UpdatedAt,
            new Version(metadataVideo.V))
        { }

        internal MetadataVideo(
            MetadataVideoPreviewSchema2 metadataVideoPreviewSchema2, 
            MetadataVideoDetailSchema2 metadataVideoDetailSchema2) 
            : this(
                metadataVideoDetailSchema2.AspectRatio,
                metadataVideoDetailSchema2.BatchId,
                metadataVideoDetailSchema2.Description,
                metadataVideoPreviewSchema2.Duration,
                metadataVideoPreviewSchema2.CreatedAt,
                metadataVideoPreviewSchema2.OwnerAddress,
                metadataVideoDetailSchema2.PersonalData,
                metadataVideoDetailSchema2.Sources.Select(s => new MetadataVideoSource(s)),
                metadataVideoPreviewSchema2.Thumbnail is null ? null : new SwarmImageRaw(metadataVideoPreviewSchema2.Thumbnail),
                metadataVideoPreviewSchema2.Title,
                metadataVideoPreviewSchema2.UpdatedAt,
                new Version(metadataVideoPreviewSchema2.V))
        { }

        // Properties.
        public float? AspectRatio { get; }
        public string? BatchId { get; }
        public string Description { get; }
        public long Duration { get; }
        public long CreatedAt { get; }
        public string? PersonalData { get; }
        public string OwnerAddress { get; }
        public IEnumerable<MetadataVideoSource> Sources { get; }
        public SwarmImageRaw? Thumbnail { get; }
        public string Title { get; }
        public long? UpdatedAt { get; }
        public Version Version { get; }
    }
}
