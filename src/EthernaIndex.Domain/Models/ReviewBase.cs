using Etherna.MongODM.Core.Attributes;
using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class ReviewBase : EntityModelBase<string>
    {
        // Constructors.
        protected ReviewBase(
            ContentReviewType contentReview,
            string description,
            string manifestHash,
            User owner)
        {
            ContentReview = contentReview;
            Description = description;
            ManifestHash = manifestHash;
            ReviewOwner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ReviewBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual ContentReviewType ContentReview { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual DateTime? LastUpdate { get; protected set; }
        public virtual string ManifestHash { get; protected set; }
        public virtual User ReviewOwner { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(ContentReview))]
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(LastUpdate))]
        public virtual void ChangeReview(
            ContentReviewType contentReview,
            string description)
        {
            ContentReview = contentReview;
            Description = description;
            LastUpdate = DateTime.UtcNow;
        }
    }
}
