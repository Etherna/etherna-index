using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;

namespace Etherna.EthernaIndex.Domain.Events
{
    public class VideoModeratedEvent : IDomainEvent
    {
        public VideoModeratedEvent(Video video)
        {
            Video = video;
        }

        public Video Video { get; }
    }
}
