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

using Etherna.EthernaIndex.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2
{
    public class VideoSourceV2 : ModelBase
    {
        // Constructors.
        public VideoSourceV2(
            string path,
            string? quality,
            long size,
            string type)
        {
            // Validate args.
            var validationErrors = new List<ValidationError>();

            //quality
            if (quality is not null &&
                string.IsNullOrWhiteSpace(quality))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty quality"));

            //path
            if (string.IsNullOrWhiteSpace(path))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty path"));

            //type
            if (string.IsNullOrWhiteSpace(type))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty type"));

            //size
            if (size <= 0 &&
                !path.EndsWith("/manifest.m3u8", StringComparison.InvariantCultureIgnoreCase))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has invalid size"));

            // Throws validation exception.
            if (validationErrors.Count != 0)
                throw new VideoManifestValidationException(validationErrors);

            // Assign properties.
            Path = path;
            Quality = quality;
            Size = size;
            Type = type;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoSourceV2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        //from v2.0
        public virtual string Path { get; set; }
        public virtual string? Quality { get; set; }
        public virtual long Size { get; set; }
        public virtual string Type { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<string>.Default.Equals(Path, (obj as VideoSourceV2)!.Path) &&
                EqualityComparer<string>.Default.Equals(Quality, (obj as VideoSourceV2)!.Quality) &&
                Size.Equals((obj as VideoSourceV2)?.Size) &&
                EqualityComparer<string>.Default.Equals(Type, (obj as VideoSourceV2)!.Type);
        }

        public override int GetHashCode() =>
            Path.GetHashCode(StringComparison.Ordinal) ^
            Quality?.GetHashCode(StringComparison.Ordinal) ?? "".GetHashCode(StringComparison.Ordinal) ^
            Size.GetHashCode() ^
            Type.GetHashCode(StringComparison.Ordinal);
    }
}
