using Etherna.EthernaIndex.Swarm.DtoModels;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class SwarmImageRaw
    {
        // Constructors.
        public SwarmImageRaw(
            float aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }
        internal SwarmImageRaw(SwarmImageRawSchema1 swarmImageRaw) :
            this(swarmImageRaw.AspectRatio,
                swarmImageRaw.Blurhash,
                swarmImageRaw.Sources)
        { }

        // Properties.
        public float AspectRatio { get; }
        public string Blurhash { get; }
        public IReadOnlyDictionary<string, string> Sources { get; }
    }
}
