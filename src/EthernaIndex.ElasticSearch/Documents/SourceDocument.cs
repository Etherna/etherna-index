namespace EthernaIndex.ElasticSearch.Documents
{
    public class SourceDocument
    {
        // Constructors.
        public SourceDocument(
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
