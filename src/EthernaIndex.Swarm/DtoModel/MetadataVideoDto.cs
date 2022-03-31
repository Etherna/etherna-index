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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MetadataVideoDto() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string? Description { get; set; }
        public float Duration { get; set; }
        public string OriginalQuality { get; set; }
        public string OwnerAddress { get; set; }
        public IEnumerable<MetadataVideoSourceDto> Sources { get; set; }
        public SwarmImageRawDto? Thumbnail { get; set; }
        public string Title { get; set; }
    }
}
