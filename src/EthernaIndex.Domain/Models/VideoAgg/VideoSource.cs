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
            int? bitrate,
            string quality,
            string reference,
            long size)
        {
            Bitrate = bitrate;
            Quality = quality;
            Reference = reference;
            Size = size;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoSource() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual int? Bitrate { get; set; }
        public virtual string Quality { get; set; }
        public virtual string Reference { get; set; }
        public virtual long Size { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<int?>.Default.Equals(Bitrate, (obj as VideoSource)!.Bitrate) &&
                EqualityComparer<string>.Default.Equals(Quality, (obj as VideoSource)!.Quality) &&
                EqualityComparer<string>.Default.Equals(Reference, (obj as VideoSource)!.Reference) &&
                Size.Equals((obj as VideoSource)?.Size);
        }

        public override int GetHashCode() =>
            (Bitrate ?? 0).GetHashCode() ^
            Quality.GetHashCode(StringComparison.Ordinal) ^
            Reference.GetHashCode(StringComparison.Ordinal) ^
            Size.GetHashCode();
    }
}