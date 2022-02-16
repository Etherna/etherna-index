using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class ImageDto
    {
        // Constructors.
        public ImageDto(
            int aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            BlurHash = blurhash;
            Sources = sources;
        }

        // Properties.
        public int AspectRatio { get; private set; }
        public string BlurHash { get; private set; }
        public IReadOnlyDictionary<string, string> Sources { get; private set; }
    }
}
