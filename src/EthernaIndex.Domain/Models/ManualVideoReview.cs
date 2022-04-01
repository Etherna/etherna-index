using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class ManualVideoReview : ManualReviewBase
    {
        // Constructors.
        public ManualVideoReview(
            User author,
            string description,
            bool isValid,
            Video video)
            : base(author, description, isValid)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            Video = video;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ManualVideoReview() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual Video Video { get; private set; }
    }
}
