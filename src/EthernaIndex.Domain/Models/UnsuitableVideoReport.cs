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

using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class UnsuitableVideoReport : UnsuitableReportBase
    {
        // Constructors.
        public UnsuitableVideoReport(
            Video video,
            VideoManifest videoManifest,
            User reporterAuthor,
            string description)
            : base(description, reporterAuthor)
        {
            Video = video ?? throw new ArgumentNullException(nameof(video));
            VideoManifest = videoManifest ?? throw new ArgumentNullException(nameof(videoManifest));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected UnsuitableVideoReport() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual Video Video { get; protected set; }
        public virtual VideoManifest VideoManifest { get; protected set; }
    }
}
