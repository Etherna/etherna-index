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

using Etherna.EthernaIndex.Domain.Models.ValidationResults;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoValidationResult : ValidationResult
    {
        // Fields.
        private List<VideoSource> _sources = new();

        // Constructors.
        public VideoValidationResult(
            string manifestHash,
            User owner)
            : base(manifestHash, owner)
        {
        }

        protected VideoValidationResult() { }

        // Properties.
        public string? FeedTopicId { get; protected set; }
        public string? Description { get; protected set; }
        public int? Duration { get; protected set; }
        public string? OriginalQuality { get; protected set; }
        public string? Title { get; protected set; }
        public SwarmImageRaw? Thumbnail { get; protected set; }
#pragma warning disable CA2227 // Collection properties should be read only
        public virtual IEnumerable<VideoSource>? Sources
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get => _sources;
            protected set => _sources = new List<VideoSource>(value ?? new List<VideoSource>());
        }

        // Methods.
        [PropertyAlterer(nameof(IsInizialized))]
        [PropertyAlterer(nameof(FeedTopicId))]
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(Duration))]
        [PropertyAlterer(nameof(OriginalQuality))]
        [PropertyAlterer(nameof(Title))]
        [PropertyAlterer(nameof(Thumbnail))]
        [PropertyAlterer(nameof(Sources))]
        public virtual void InizializeManifest(
            string feedTopicId,
            string title,
            string description,
            string originalQuality,
            int duration,
            SwarmImageRaw? thumbnail,
            IEnumerable<VideoSource>? sources)
        {
            FeedTopicId = feedTopicId;
            Title = title;
            Description = description;
            OriginalQuality = originalQuality;
            Duration = duration;
            //Thumbnail = thumbnail;
            //Sources = sources;
            IsInizialized = true;
        }
    }
}
