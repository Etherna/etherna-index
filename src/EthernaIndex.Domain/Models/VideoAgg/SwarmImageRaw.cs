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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class SwarmImageRaw : ModelBase
    {
        // Fields.
        private List<ImageSource> _sources = new();

        // Constructors.
        public SwarmImageRaw(
            float aspectRatio,
            string blurhash,
            IEnumerable<ImageSource> sourcesV2)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            _sources.AddRange(sourcesV2);
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected SwarmImageRaw() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual float AspectRatio { get; set; }
        public virtual string Blurhash { get; set; }
        public virtual IEnumerable<ImageSource> SourcesV2 //should return to be "Sources". Required by db migration, see: https://etherna.atlassian.net/browse/MODM-161
        {
            get => _sources;
            protected set => _sources = new List<ImageSource>(value ?? Array.Empty<ImageSource>());
        }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                AspectRatio.Equals((obj as SwarmImageRaw)?.AspectRatio) &&
                EqualityComparer<string>.Default.Equals(Blurhash, (obj as SwarmImageRaw)!.Blurhash) &&
                SourcesV2.Count() == (obj as SwarmImageRaw)?.SourcesV2.Count() && 
                !SourcesV2.Except(((SwarmImageRaw)obj).SourcesV2).Any();
        }

        public override int GetHashCode() =>
            AspectRatio.GetHashCode() ^
            Blurhash.GetHashCode(StringComparison.Ordinal) ^
            SourcesV2.GetHashCode();
    }
}
