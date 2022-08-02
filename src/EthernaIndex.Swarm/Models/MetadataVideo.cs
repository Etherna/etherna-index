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
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class MetadataVideo
    {
        // Constructors.
        public MetadataVideo(
            string description,
            long duration,
            long createdAt,
            string originalQuality,
            string ownerAddress,
            IEnumerable<MetadataVideoSource> sources,
            SwarmImageRaw? thumbnail,
            string title)
        {
            Description = description;
            Duration = duration;
            CreatedAt = createdAt;
            OriginalQuality = originalQuality;
            OwnerAddress = ownerAddress;
            Sources = sources;
            Thumbnail = thumbnail;
            Title = title;
        }
        internal MetadataVideo(MetadataVideoSchema1 metadataVideo) :
            this(metadataVideo.Description,
                metadataVideo.Duration,
                metadataVideo.CreatedAt,
                metadataVideo.OriginalQuality,
                metadataVideo.OwnerAddress,
                metadataVideo.Sources.Select(s => new MetadataVideoSource(s)),
                metadataVideo.Thumbnail is null ? null : new SwarmImageRaw(metadataVideo.Thumbnail),
                metadataVideo.Title)
        { }

        // Properties.
        public string Description { get; }
        public long Duration { get; }
        public long CreatedAt { get; }
        public string OriginalQuality { get; }
        public string OwnerAddress { get; }
        public IEnumerable<MetadataVideoSource> Sources { get; }
        public SwarmImageRaw? Thumbnail { get; }
        public string Title { get; }
    }
}
