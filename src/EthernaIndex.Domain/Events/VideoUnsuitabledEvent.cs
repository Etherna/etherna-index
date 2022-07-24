using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;

namespace Etherna.EthernaIndex.Domain.Events
{
    public class VideoUnsuitabledEvent : IDomainEvent
    {
        public VideoUnsuitabledEvent(Video entity)
        {
            Entity = entity;
        }

        public Video Entity { get; }
    }
}
