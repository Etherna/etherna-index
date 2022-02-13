using Etherna.MongODM.Core.Attributes;
using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class ReportBase : EntityModelBase<string>
    {
        // Constructors.
        protected ReportBase(
            string description,
            User owner) 
        {
            Description = description;
            ReporterOwner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ReportBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string Description { get; protected set; }
        public virtual DateTime? LastUpdate { get; protected set; }
        public virtual User ReporterOwner { get; protected set; } = default!;

        // Methods.
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(LastUpdate))]
        public virtual void ChangeDescription(string description)
        {
            Description = description;
            LastUpdate = DateTime.UtcNow;
        }
    }
}
