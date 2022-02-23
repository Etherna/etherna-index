using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class ImageDto
    {
        // Constructors.
        public ImageDto(
            float aspectRatio,
            string blurHash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            BlurHash = blurHash;
            Sources = sources;
        }

        // Properties.
        public float AspectRatio { get; private set; }
        public string BlurHash { get; private set; }
        public IReadOnlyDictionary<string, string> Sources { get; private set; }
    }
}
