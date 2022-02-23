using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class ManualReviewBase : EntityModelBase<string>
    {
        // Constructors.
        protected ManualReviewBase(
            User author,
            string description,
            bool isValid)
        {
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Description = description;
            IsValid = isValid;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ManualReviewBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual User Author { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual bool IsValid { get; protected set; }
    }
}
