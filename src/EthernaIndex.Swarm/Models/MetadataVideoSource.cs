using Etherna.EthernaIndex.Swarm.DtoModels;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class MetadataVideoSource
    {
        // Constructors.
        public MetadataVideoSource(
            int bitrate,
            string quality,
            string reference,
            long size)
        {
            Bitrate = bitrate;
            Quality = quality;
            Reference = reference;
            Size = size;
        }
        internal MetadataVideoSource(MetadataVideoSourceSchema1 metadataVideoSource) :
            this(metadataVideoSource.Bitrate,
                metadataVideoSource.Quality,
                metadataVideoSource.Reference,
                metadataVideoSource.Size)
        { }

        // Properties.
        public int Bitrate { get; }
        public string Quality { get; }
        public string Reference { get; }
        public long Size { get; }
    }
}
