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
    public class VideoReport : ReportBase
    {
        // Constructors.
        public VideoReport(
            VideoManifest videoManifest,
            User owner,
            string description)
            : base(description, owner)
        {
            VideoManifest = videoManifest ?? throw new ArgumentNullException(nameof(videoManifest));
            Video = videoManifest.Video;
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoReport() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        // Properties.
        public virtual Video Video { get; protected set; }
        public virtual VideoManifest VideoManifest { get; protected set; }
    }
}
