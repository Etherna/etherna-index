using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class ImageSource : ModelBase
    {
        // Constructors.
        public ImageSource(
            int width,
            string? type,
            string path)
        {
            Width = width;
            Type = type;
            Path = path;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ImageSource() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string Path { get; set; }
        public virtual string? Type { get; set; }
        public virtual int Width { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<int?>.Default.Equals(Width, (obj as ImageSource)!.Width) &&
                EqualityComparer<string>.Default.Equals(Type, (obj as ImageSource)!.Type) &&
                EqualityComparer<string>.Default.Equals(Path, (obj as ImageSource)!.Path);
        }

        public override int GetHashCode() =>
            Path?.GetHashCode(StringComparison.Ordinal) ?? "".GetHashCode(StringComparison.Ordinal) ^
            Type?.GetHashCode(StringComparison.Ordinal) ?? "".GetHashCode(StringComparison.Ordinal) ^
            Width.GetHashCode();
    }
}