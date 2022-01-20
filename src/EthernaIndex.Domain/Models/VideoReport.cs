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
    public class VideoReport : EntityModelBase<string>
    {
        // Constructors and dispose.
        public VideoReport(
            Video video,
            User owner,
            string description)
        {
            ReporterOwner = owner ?? throw new ArgumentNullException(nameof(owner));
            Video = video ?? throw new ArgumentNullException(nameof(video));
            ReportDescription = description ?? throw new ArgumentNullException(nameof(description));
        }
        protected VideoReport() { }


        // Properties.
        public virtual bool? ContentApproved { get; protected set; }
        public virtual string ReportDescription { get; protected set; } = default!;
        public virtual DateTime? LastCheck { get; protected set; }
        public virtual User ReporterOwner { get; protected set; } = default!;
        public virtual Video Video { get; protected set; } = default!;

        // Methods.
        [PropertyAlterer(nameof(ContentApproved))]
        [PropertyAlterer(nameof(LastCheck))]
        public void ApproveContent()
        {
            ContentApproved = true;
            LastCheck = DateTime.UtcNow;
            Video.ContentApproved = true; //TODO what is the PropertyAlterer
        }

        [PropertyAlterer(nameof(ContentApproved))]
        [PropertyAlterer(nameof(LastCheck))]
        public void RejectContent()
        {
            ContentApproved = false;
            LastCheck = DateTime.UtcNow;
            Video.ContentApproved = false; //TODO what is the PropertyAlterer
        }
    }
}
