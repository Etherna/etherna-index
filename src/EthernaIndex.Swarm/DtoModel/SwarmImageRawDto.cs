using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class SwarmImageRawDto
    {
        public int AspectRatio { get; set; }
        public string Blurhash { get; set; } = default!;
        public IReadOnlyDictionary<string, string> Sources { get; set; } = default!;
    }
}
