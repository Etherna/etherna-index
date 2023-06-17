using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Domain.Events
{
    public class ManualVideoReviewRejectedEvent : IDomainEvent
    {
        public ManualVideoReviewRejectedEvent(Video video)
        {
            Video = video;
        }

        public Video Video { get; }
    }
}
