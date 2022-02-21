using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class UnsuitableReviewBase : EntityModelBase<string>
    {
        // Constructors.
        protected UnsuitableReviewBase(
            ContentReviewStatus contentReview,
            string description,
            string manifestHash,
            User reviewAuthor)
        {
            ContentReview = contentReview;
            Description = description;
            ManifestHash = manifestHash;
            ReviewAuthor = reviewAuthor ?? throw new ArgumentNullException(nameof(reviewAuthor));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected UnsuitableReviewBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual ContentReviewStatus ContentReview { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual DateTime? LastUpdate { get; protected set; }
        public virtual string ManifestHash { get; protected set; }
        public virtual User ReviewAuthor { get; protected set; }
    }
}
