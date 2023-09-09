//   Copyright 2021-present Etherna Sagl
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

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
        public virtual bool IsArchived { get; protected set; }
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

        [PropertyAlterer(nameof(IsArchived))]
        public virtual void SetArchived()
        {
            IsArchived = true;
        }
    }
}
