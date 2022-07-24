using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;

namespace Etherna.EthernaIndex.Domain.Events
{
    public class ManifestSuccessfulValidatedEvent : IDomainEvent
    {
        public ManifestSuccessfulValidatedEvent(VideoManifest entity, string videoId)
        {
            Entity = entity;
            VideoId = videoId;
        }

        public VideoManifest Entity { get; }
        public string VideoId { get; }
    }
}
