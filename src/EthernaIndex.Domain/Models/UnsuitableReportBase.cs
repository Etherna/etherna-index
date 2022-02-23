using Etherna.MongODM.Core.Attributes;
using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class UnsuitableReportBase : EntityModelBase<string>
    {
        // Constructors.
        protected UnsuitableReportBase(
            string description,
            User reporterAuthor) 
        {
            Description = description;
            ReporterAuthor = reporterAuthor ?? throw new ArgumentNullException(nameof(reporterAuthor));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected UnsuitableReportBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string Description { get; protected set; }
        public virtual DateTime? LastUpdate { get; protected set; }
        public virtual User ReporterAuthor { get; protected set; }

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
