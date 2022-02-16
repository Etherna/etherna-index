﻿using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class SwarmImageRawDto
    {
        // Constructors.
        public SwarmImageRawDto(
            int aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }

        // Properties.
        public int AspectRatio { get; set; }
        public string Blurhash { get; set; }
        public IReadOnlyDictionary<string, string> Sources { get; set; }
    }
}
