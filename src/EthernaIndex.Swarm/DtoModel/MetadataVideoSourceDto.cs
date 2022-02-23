namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class MetadataVideoSourceDto
    {
        // Constructors.
        public MetadataVideoSourceDto(
            int? bitrate,
            string quality,
            string reference,
            int? size)
        {
            Bitrate = bitrate;
            Quality = quality;
            Reference = reference;
            Size = size;
        }

        // Properties.
        public int? Bitrate { get; set; }
        public string Quality { get; set; }
        public string Reference { get; set; }
        public int? Size { get; set; }
    }
}
