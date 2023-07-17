using Etherna.EthernaIndex.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2
{
    public class ImageSourceV2 : ModelBase
    {
        // Constructors.
        public ImageSourceV2(
            int width,
            string path,
            string type)
        {
            // Validate args.
            var validationErrors = new List<ValidationError>();

            //width
            if (width <= 0)
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has wrong width"));

            //path
            if (string.IsNullOrWhiteSpace(path))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has empty path"));

            //type
            if (string.IsNullOrWhiteSpace(type))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidThumbnailSource, $"Thumbnail has empty type"));

            // Throws validation exception.
            if (validationErrors.Any())
                throw new VideoManifestValidationException(validationErrors);

            // Assign properties.
            Width = width;
            Type = type;
            Path = path;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ImageSourceV2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string Path { get; set; }
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
                EqualityComparer<string>.Default.Equals(Path, (obj as ImageSourceV2)!.Path);
        }

        public override int GetHashCode() =>
            Path.GetHashCode(StringComparison.Ordinal) ^
            Type.GetHashCode(StringComparison.Ordinal) ^
            Width.GetHashCode();
    }
}