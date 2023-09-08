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

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1
{
    public class VideoSourceV1 : ModelBase
    {
        // Constructors.
        public VideoSourceV1(
            int? bitrate,
            string quality,
            string reference,
            long? size)
        {
            // Validate args.
            var validationErrors = new List<ValidationError>();

            //quality
            if (string.IsNullOrWhiteSpace(quality))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty quality"));

            //reference
            if (string.IsNullOrWhiteSpace(reference))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Video source has empty reference"));

            // Throws validation exception.
            if (validationErrors.Any())
                throw new VideoManifestValidationException(validationErrors);

            // Assign properties.
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
        public virtual string Reference { get; set; }
        public virtual long? Size { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<int?>.Default.Equals(Bitrate, (obj as VideoSourceV1)!.Bitrate) &&
                EqualityComparer<string>.Default.Equals(Quality, (obj as VideoSourceV1)!.Quality) &&
                EqualityComparer<string>.Default.Equals(Reference, (obj as VideoSourceV1)!.Reference) &&
                Size.Equals((obj as VideoSourceV1)?.Size);
        }

        public override int GetHashCode() =>
            Bitrate.GetHashCode() ^
            Quality.GetHashCode(StringComparison.Ordinal) ^
            Reference.GetHashCode(StringComparison.Ordinal) ^
            Size.GetHashCode();
    }
}
