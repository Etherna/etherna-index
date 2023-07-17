namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class ImageSourceDto
    {
        // Constructors.
        public ImageSourceDto(
            string? type,
            string path,
            int width)
        {
            Type = type;
            Path = path;
            Width = width;
        }

        // Properties.
        public string? Type { get; private set; }
        public string Path { get; private set; }
        public int Width { get; private set; }
    }
}
