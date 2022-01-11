namespace Etherna.EthernaIndex.Domain.Models.Meta
{
    public class MetadataVideoSource
    {
        // Constructors.
        protected MetadataVideoSource() { }

        // Properties.
        public string Quality { get; set; } = default!;
        public string Reference { get; set; } = default!;
        public int? Size { get; set; }
        public int? Bitrate { get; set; }
    }
}
