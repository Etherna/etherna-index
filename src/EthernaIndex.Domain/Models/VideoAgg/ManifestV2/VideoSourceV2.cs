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
            if (validationErrors.Any())
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
