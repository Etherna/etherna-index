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

using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.ManifestAgg
{
    public class SwarmImageRaw : ModelBase
    {
        // Fields.
        private Dictionary<string, string> _sources = new();

        // Constructors.
        public SwarmImageRaw(
            float aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string>? sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources ?? new Dictionary<string, string>();
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected SwarmImageRaw() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual float AspectRatio { get; set; }
        public virtual string Blurhash { get; set; }
        public virtual IReadOnlyDictionary<string, string> Sources
        {
            get => _sources;
            protected set => _sources = new Dictionary<string, string>(value ?? new Dictionary<string, string>());
        }

        // Methods.
        [PropertyAlterer(nameof(Sources))]
        public virtual void AddSource(string sourceType, string reference)
        {
            _sources[sourceType] = reference;
        }

        [PropertyAlterer(nameof(Sources))]
        public virtual void DeleteSource(string sourceType, string reference)
        {
            _sources.Remove(sourceType);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                AspectRatio.Equals((obj as SwarmImageRaw)?.AspectRatio) &&
                EqualityComparer<string>.Default.Equals(Blurhash, (obj as SwarmImageRaw)!.Blurhash) &&
                Sources.Equals((obj as SwarmImageRaw)?.Sources);
        }

        public override int GetHashCode() =>
            AspectRatio.GetHashCode() ^
            Blurhash.GetHashCode(StringComparison.Ordinal) ^
            Sources.GetHashCode();
    }
}
