using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.Domain.Events
{
    public class ManualVideoReviewRejectedEvent : IDomainEvent
    {
        // Constructors.
        public ManualVideoReviewRejectedEvent(Video video)
        {
            Video = video ?? throw new ArgumentNullException(nameof(video));
        }

        // Properties.
        public Video Video { get; }
    }
}
