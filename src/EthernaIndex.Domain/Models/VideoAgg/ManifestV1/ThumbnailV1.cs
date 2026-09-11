// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.SwarmSdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1
{
    public class ThumbnailV1 : ModelBase
    {
        // Fields.
        private Dictionary<string, SwarmReference> _sources = new();

        // Constructors.
        public ThumbnailV1(
            float aspectRatio,
            string blurhash,
            IDictionary<string, SwarmReference> sources)
        {
            ArgumentNullException.ThrowIfNull(sources);
            
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            foreach (var s in sources)
                _sources.Add(s.Key, s.Value);
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ThumbnailV1() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual float AspectRatio { get; set; }
        public virtual string Blurhash { get; set; }
        public virtual IReadOnlyDictionary<string, SwarmReference> Sources
        {
            get => _sources;
            protected set => _sources = new Dictionary<string, SwarmReference>(value ?? new Dictionary<string, SwarmReference>());
        }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                AspectRatio.Equals((obj as ThumbnailV1)?.AspectRatio) &&
                EqualityComparer<string>.Default.Equals(Blurhash, (obj as ThumbnailV1)!.Blurhash) &&
                Sources.Count == (obj as ThumbnailV1)?.Sources?.Count && !Sources.Except(((ThumbnailV1)obj).Sources).Any();
        }

        public override int GetHashCode() =>
            AspectRatio.GetHashCode() ^
            Blurhash.GetHashCode(StringComparison.Ordinal) ^
            Sources.GetHashCode();
    }
}
