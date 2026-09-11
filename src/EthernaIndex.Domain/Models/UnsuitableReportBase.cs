// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

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
        public virtual bool IsArchived { get; protected set; }
        public virtual DateTime? LastUpdate { get; protected set; }
        public virtual User ReporterAuthor { get; protected set; }

        // Methods.
        public virtual void ChangeDescription(string description)
        {
            Description = description;
            LastUpdate = DateTime.UtcNow;
        }

        public virtual void SetArchived()
        {
            IsArchived = true;
        }
    }
}
