using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class ImageSource : ModelBase
    {
        // Constructors.
        public ImageSource(
            int width,
            string type,
            string? path,
            string? reference)
        {
            Width = width;
            Type = type;
            Path = path;
            Reference = reference;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ImageSource() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual int Width { get; set; }
        public virtual string Type { get; set; }
        public virtual string? Path { get; set; }
        public virtual string? Reference { get; set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<int?>.Default.Equals(Width, (obj as ImageSource)!.Width) &&
                EqualityComparer<string>.Default.Equals(Type, (obj as ImageSource)!.Type) &&
                EqualityComparer<string>.Default.Equals(Path, (obj as ImageSource)!.Path) &&
                EqualityComparer<string>.Default.Equals(Reference, (obj as ImageSource)!.Reference);
        }

        public override int GetHashCode() =>
            Width.GetHashCode() ^
            Type.GetHashCode(StringComparison.Ordinal); //TODO add Path and Reference
    }
}