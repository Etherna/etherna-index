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

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1
{
    public class VideoSourceV1 : ModelBase
    {
        // Constructors.
        public VideoSourceV1(
            int? bitrate,
            string quality,
            SwarmReference reference,
            long? size)
        {
            Bitrate = bitrate;
            Quality = quality;
            Reference = reference;
            Size = size;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoSourceV1() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        //from v1.0
        public virtual int? Bitrate { get; set; }
        public virtual string Quality { get; set; }
        public virtual SwarmReference Reference { get; set; }
        public virtual long? Size { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<int?>.Default.Equals(Bitrate, (obj as VideoSourceV1)!.Bitrate) &&
                EqualityComparer<string>.Default.Equals(Quality, (obj as VideoSourceV1)!.Quality) &&
                EqualityComparer<SwarmReference>.Default.Equals(Reference, (obj as VideoSourceV1)!.Reference) &&
                Size.Equals((obj as VideoSourceV1)?.Size);
        }

        public override int GetHashCode() =>
            Bitrate.GetHashCode() ^
            Quality.GetHashCode(StringComparison.Ordinal) ^
            Reference.GetHashCode() ^
            Size.GetHashCode();
    }
}
