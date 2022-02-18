using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoReview : ReviewBase
    {
        // Constructors.
        public VideoReview(
            ContentReviewStatus contentReview,
            string description,
            string manifestHash,
            User reviewAuthor,
            string videoId) 
            : base(contentReview, description, manifestHash, reviewAuthor) 
        {
            if (videoId is null)
                throw new ArgumentNullException(nameof(videoId));
            VideoId = videoId;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoReview() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string VideoId { get; private set; }
    }
}
