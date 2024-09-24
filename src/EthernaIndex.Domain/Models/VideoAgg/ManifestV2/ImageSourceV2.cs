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

using Etherna.BeeNet.Models;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2
{
    public class ImageSourceV2 : ModelBase
    {
        // Constructors.
        public ImageSourceV2(
            int width,
            SwarmUri path,
            string type)
        {
            Width = width;
            Type = type;
            Path = path;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ImageSourceV2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual SwarmUri Path { get; set; }
        public virtual string Type { get; set; }
        public virtual int Width { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<int?>.Default.Equals(Width, (obj as ImageSourceV2)!.Width) &&
                EqualityComparer<string>.Default.Equals(Type, (obj as ImageSourceV2)!.Type) &&
                EqualityComparer<SwarmUri>.Default.Equals(Path, (obj as ImageSourceV2)!.Path);
        }

        public override int GetHashCode() =>
            Path.GetHashCode() ^
            Type.GetHashCode(StringComparison.Ordinal) ^
            Width.GetHashCode();
    }
}