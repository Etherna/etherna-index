using System.Collections.Generic;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class ImageDocument
    {
        // Constructors.
        public ImageDocument(
            float aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }

        // Properties.
        public float AspectRatio { get; private set; }
        public string Blurhash { get; private set; }
        public IReadOnlyDictionary<string, string> Sources { get; private set; }
    }
}
