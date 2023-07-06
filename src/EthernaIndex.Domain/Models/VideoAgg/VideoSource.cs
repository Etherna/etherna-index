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

using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class VideoSource : ModelBase
    {
        // Constructors.
        public VideoSource(
            string path,
            string quality,
            long size,
            string type)
        {
            Path = path;
            Quality = quality;
            Size = size;
            Type = type;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoSource() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string Path { get; set; } // from for v2.
        public virtual string Quality { get; set; }
        public virtual long Size { get; set; }
        public virtual string Type { get; set; } // from for v2.

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<string>.Default.Equals(Path, (obj as VideoSource)!.Path) &&
                EqualityComparer<string>.Default.Equals(Quality, (obj as VideoSource)!.Quality) &&
                Size.Equals((obj as VideoSource)?.Size) &&
                EqualityComparer<string>.Default.Equals(Type, (obj as VideoSource)!.Type);
        }

        public override int GetHashCode() =>
            Path?.GetHashCode(StringComparison.Ordinal) ?? "".GetHashCode(StringComparison.Ordinal) ^
            Quality.GetHashCode(StringComparison.Ordinal) ^
            Size.GetHashCode() ^
            Type?.GetHashCode(StringComparison.Ordinal) ?? "".GetHashCode(StringComparison.Ordinal);
    }
}
