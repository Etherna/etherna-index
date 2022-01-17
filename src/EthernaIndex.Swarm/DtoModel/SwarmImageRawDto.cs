using System.Collections.Generic;

namespace EthernaIndex.Swarm.DtoModel
{
    public class SwarmImageRawDto
    {
        public int AspectRatio { get; set; }
        public string Blurhash { get; set; } = default!;
        public Dictionary<string, string> Sources { get; set; } = default!;
    }
}
