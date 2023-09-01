namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoSource2Dto
    {
        // Constructors.
        public VideoSource2Dto(
            string type,
            string? quality,
            string path,
            long size)
        {
            Type = type;
            Quality = quality;
            Path = path;
            Size = size;
        }

        // Properties.
        public string Type { get; private set; }
        public string? Quality { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
    }
}
