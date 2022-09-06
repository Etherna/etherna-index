using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;

namespace Etherna.EthernaIndex.Domain.Events
{
    public class ManifestSuccessfulValidatedEvent : IDomainEvent
    {
        public ManifestSuccessfulValidatedEvent(Video video, VideoManifest videoManifest)
        {
            Video = video;
            VideoManifest = videoManifest;
        }

        public Video Video { get; }
        public VideoManifest VideoManifest { get; }
    }
}
