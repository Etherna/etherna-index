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
    public class ReportVideo : EntityModelBase<string>
    {
        // Constructors and dispose.
        public ReportVideo(
            Video video,
            User owner)
        {
            ReporterOwner = owner ?? throw new ArgumentNullException(nameof(owner));
            Video = video ?? throw new ArgumentNullException(nameof(video));
        }
        protected ReportVideo() { }


        // Properties.
        public virtual User? CheckedBy { get; protected set; }
        public virtual bool? ContentApproved { get; protected set; }
        public virtual string? Description { get; protected set; }
        public virtual DateTime? LastCheck { get; protected set; }
        public virtual User ReporterOwner { get; protected set; } = default!;
        public virtual Video Video { get; protected set; } = default!;

        // Methods.
        [PropertyAlterer(nameof(CheckedBy))]
        [PropertyAlterer(nameof(ContentApproved))]
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(LastCheck))]
        public void ApproveContent(
            User checkedBy,
            string? description)
        {
            CheckedBy = checkedBy ?? throw new ArgumentNullException(nameof(checkedBy));
            ContentApproved = true;
            Description = description;
            LastCheck = DateTime.UtcNow;
        }

        [PropertyAlterer(nameof(CheckedBy))]
        [PropertyAlterer(nameof(ContentApproved))]
        [PropertyAlterer(nameof(Description))]
        [PropertyAlterer(nameof(LastCheck))]
        public void RejectContent(
            User checkedBy,
            string? description)
        {
            CheckedBy = checkedBy ?? throw new ArgumentNullException(nameof(checkedBy));
            ContentApproved = false;
            Description = description;
            LastCheck = DateTime.UtcNow;
        }
    }
}
