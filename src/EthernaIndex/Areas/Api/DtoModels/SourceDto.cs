namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class SourceDto
    {
        // Constructors.
        public SourceDto(
            int? bitrate,
            string quality,
            string reference,
            long size)
        {
            Bitrate = bitrate;
            Quality = quality;
            Reference = reference;
            Size = size;
        }

        // Properties.
        public int? Bitrate { get; private set; }
        public string Quality { get; private set; }
        public string Reference { get; private set; }
        public long Size { get; private set; }
    }
}
