using Etherna.EthernaIndex.Swarm.DtoModels;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class MetadataVideo
    {
        // Constructors.
        public MetadataVideo(
            string description,
            long duration,
            long createdAt,
            string originalQuality,
            string ownerAddress,
            IEnumerable<MetadataVideoSource> sources,
            SwarmImageRaw? thumbnail,
            string title)
        {
            Description = description;
            Duration = duration;
            CreatedAt = createdAt;
            OriginalQuality = originalQuality;
            OwnerAddress = ownerAddress;
            Sources = sources;
            Thumbnail = thumbnail;
            Title = title;
        }
        internal MetadataVideo(MetadataVideoSchema1 metadataVideo) :
            this(metadataVideo.Description,
                metadataVideo.Duration,
                metadataVideo.CreatedAt,
                metadataVideo.OriginalQuality,
                metadataVideo.OwnerAddress,
                metadataVideo.Sources.Select(s => new MetadataVideoSource(s)),
                metadataVideo.Thumbnail is null ? null : new SwarmImageRaw(metadataVideo.Thumbnail),
                metadataVideo.Title)
        { }

        // Properties.
        public string Description { get; }
        public long Duration { get; }
        public long CreatedAt { get; }
        public string OriginalQuality { get; }
        public string OwnerAddress { get; }
        public IEnumerable<MetadataVideoSource> Sources { get; }
        public SwarmImageRaw? Thumbnail { get; }
        public string Title { get; }
    }
}
