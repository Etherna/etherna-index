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
using System.Diagnostics.CodeAnalysis;
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
            if (validationErrors.Count != 0)
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
        [SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code")]
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
