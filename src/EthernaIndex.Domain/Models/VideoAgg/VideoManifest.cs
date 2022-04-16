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

using Etherna.MongODM.Core.Attributes;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class VideoManifest : ManifestBase
    {
        // Fields.
        private List<VideoSource> _sources = new();

        // Constructors.
        public VideoManifest(string manifestHash)
            : base(manifestHash) { }
        protected VideoManifest() { }

        // Properties.
        public virtual string? Description { get; protected set; }
        public virtual float? Duration { get; protected set; }
        public virtual string? OriginalQuality { get; protected set; }
        public virtual IEnumerable<VideoSource> Sources
        {
            get => _sources;
            protected set => _sources = new List<VideoSource>(value ?? new List<VideoSource>());
        }
        public virtual SwarmImageRaw? Thumbnail { get; protected set; }
        public virtual string? Title { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(Duration))]
        [PropertyAlterer(nameof(OriginalQuality))]
        [PropertyAlterer(nameof(Sources))]
        [PropertyAlterer(nameof(Thumbnail))]
        [PropertyAlterer(nameof(Title))]
        internal virtual void SuccessfulValidation(
            string? description,
            float duration,
            string originalQuality,
            string title,
            SwarmImageRaw? thumbnail,
            IEnumerable<VideoSource> videoSources)
        {
            base.SuccessfulValidation();
            Description = description;
            Duration = duration;
            OriginalQuality = originalQuality;
            Sources = videoSources;
            Thumbnail = thumbnail;
            Title = title;
        }
    }
}
