using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.DtoModel
{
    public class MetadataVideoDto
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string OriginalQuality { get; set; } = default!;
        public string OwnerAddress { get; set; } = default!;
        public int Duration { get; set; }
        public SwarmImageRawDto Thumbnail { get; set; } = default!;
        public IEnumerable<MetadataVideoSourceDto> Sources { get; set; } = default!;
    }
}
