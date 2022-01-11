using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.Meta
{
    public class SwarmImageRaw
    {
        // Constructors.
        protected SwarmImageRaw() { }

        // Properties.
        public int AspectRatio { get; set; }
        public string Blurhash { get; set; } = default!;
        public IEnumerable<string> Sources { get; set; } = default!;
    }
}
