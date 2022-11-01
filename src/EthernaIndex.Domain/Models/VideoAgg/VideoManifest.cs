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
        // Consts.
        public const int DescriptionMaxLength = 5000;
        public const int PersonalDataMaxLength = 200;
        public const int TitleMaxLength = 200;

        // Fields.
        private List<VideoSource> _sources = new();

        // Constructors.
        public VideoManifest(string manifestHash)
            : base(manifestHash) { }
        protected VideoManifest() { }

        // Properties.
        public virtual string? BatchId { get; protected set; }
        public virtual string? Description { get; protected set; }
        public virtual long? Duration { get; protected set; }
        public virtual string? OriginalQuality { get; protected set; }
        public virtual string? PersonalData { get; protected set; }
        public virtual IEnumerable<VideoSource> Sources
        {
            get => _sources;
            protected set => _sources = new List<VideoSource>(value ?? new List<VideoSource>());
        }
        public virtual SwarmImageRaw? Thumbnail { get; protected set; }
        public virtual string? Title { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(BatchId))]
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(Duration))]
        [PropertyAlterer(nameof(OriginalQuality))]
        [PropertyAlterer(nameof(PersonalData))]
        [PropertyAlterer(nameof(Sources))]
        [PropertyAlterer(nameof(Thumbnail))]
        [PropertyAlterer(nameof(Title))]
        internal virtual void SucceededValidation(
            string? batchId,
            string description,
            long duration,
            string originalQuality,
            string? personalData,
            IEnumerable<VideoSource> sources,
            SwarmImageRaw? thumbnail,
            string title)
        {
            base.SucceededValidation();
            BatchId = batchId;
            Description = description;
            Duration = duration;
            OriginalQuality = originalQuality;
            PersonalData = personalData;
            Sources = sources;
            Thumbnail = thumbnail;
            Title = title;
        }
    }
}
