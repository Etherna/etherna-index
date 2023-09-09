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
    public class ThumbnailV2 : ModelBase
    {
        // Fields.
        private List<ImageSourceV2> _sources = new();

        // Constructors.
        public ThumbnailV2(
            float aspectRatio,
            string blurhash,
            IEnumerable<ImageSourceV2> sources)
        {
            // Validate args.
            var validationErrors = new List<ValidationError>();

            //sources
            if (sources is null || !sources.Any())
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, "Thumbnail has missing sources"));

            // Throws validation exception.
            if (validationErrors.Any())
                throw new VideoManifestValidationException(validationErrors);

            // Assign properties.
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            _sources.AddRange(sources!);
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ThumbnailV2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual float AspectRatio { get; set; }
        public virtual string Blurhash { get; set; }
        public virtual IEnumerable<ImageSourceV2> Sources
        {
            get => _sources;
            protected set => _sources = new List<ImageSourceV2>(value ?? Array.Empty<ImageSourceV2>());
        }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                AspectRatio.Equals((obj as ThumbnailV2)?.AspectRatio) &&
                EqualityComparer<string>.Default.Equals(Blurhash, (obj as ThumbnailV2)!.Blurhash) &&
                Sources.Count() == (obj as ThumbnailV2)?.Sources?.Count() && !Sources.Except(((ThumbnailV2)obj).Sources).Any();
        }

        public override int GetHashCode() =>
            AspectRatio.GetHashCode() ^
            Blurhash.GetHashCode(StringComparison.Ordinal) ^
            Sources.GetHashCode();
    }
}
