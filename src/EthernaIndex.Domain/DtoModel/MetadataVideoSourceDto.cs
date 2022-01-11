namespace Etherna.EthernaIndex.Domain.DtoModel
{
    public class MetadataVideoSourceDto
    {
        public string Quality { get; set; } = default!;
        public string Reference { get; set; } = default!;
        public int? Size { get; set; }
        public int? Bitrate { get; set; }
    }
}
