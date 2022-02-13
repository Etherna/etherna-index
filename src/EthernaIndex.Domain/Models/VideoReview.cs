using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoReview : ReviewBase
    {
        // Constructors.
        public VideoReview(
            ContentReviewType contentReview,
            string description,
            string manifestHash,
            User owner,
            Video video) 
            : base(contentReview, description, manifestHash, owner) 
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            Video = video;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoReview() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual Video Video { get; private set; }
    }
}
