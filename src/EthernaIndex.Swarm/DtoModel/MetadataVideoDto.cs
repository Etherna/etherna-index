using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.DtoModel
{
    public class MetadataVideoDto
    {
        // Constructors.
        public MetadataVideoDto(
            string id,
            string? title,
            string? description,
            string? originalQuality,
            string? ownerAddress,
            int? duration,
            SwarmImageRawDto? thumbnail,
            IEnumerable<MetadataVideoSourceDto>? sources)
        {
            Id = id;
            Title = title;
            Description = description;
            OriginalQuality = originalQuality;
            OwnerAddress = ownerAddress;
            Duration = duration;
            Thumbnail = thumbnail;
            Sources = sources;
        }

        // Properties.
        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? OriginalQuality { get; set; }
        public string? OwnerAddress { get; set; }
        public int? Duration { get; set; }
        public SwarmImageRawDto? Thumbnail { get; set; }
        public IEnumerable<MetadataVideoSourceDto>? Sources { get; set; }
    }
}
