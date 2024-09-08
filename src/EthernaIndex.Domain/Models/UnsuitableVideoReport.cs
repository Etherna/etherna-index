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
