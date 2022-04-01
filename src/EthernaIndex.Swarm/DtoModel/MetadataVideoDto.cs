using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class MetadataVideoDto
    {
        // Constructors.
        public MetadataVideoDto(
            string title,
            string? description,
            string originalQuality,
            string ownerAddress,
            float duration,
            SwarmImageRawDto? thumbnail,
            IEnumerable<MetadataVideoSourceDto> sources)
        {
            Title = title;
            Description = description;
            OriginalQuality = originalQuality;
            OwnerAddress = ownerAddress;
            Duration = duration;
            Thumbnail = thumbnail;
            Sources = sources;
        }
        public MetadataVideoDto() { }

        // Properties.
        public string? Description { get; set; }
        public float? Duration { get; set; }
        public string? OriginalQuality { get; set; }
        public string? OwnerAddress { get; set; }
        public IEnumerable<MetadataVideoSourceDto>? Sources { get; set; }
        public SwarmImageRawDto? Thumbnail { get; set; }
        public string? Title { get; set; }
    }
}
