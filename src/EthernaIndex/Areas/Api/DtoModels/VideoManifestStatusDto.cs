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

using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoManifestStatusDto
    {
        // Constructors.
        public VideoManifestStatusDto(
            Video video,
            VideoManifest videoManifest)
        {
            ArgumentNullException.ThrowIfNull(video, nameof(video));
            ArgumentNullException.ThrowIfNull(videoManifest, nameof(videoManifest));

            if (!video.VideoManifests.Contains(videoManifest))
                throw new InvalidOperationException("Video must contain the manifest");

            ErrorDetails = videoManifest.ValidationErrors
                .Select(i => new ErrorDetailDto(i.ErrorMessage, i.ErrorType));
            Hash = videoManifest.ManifestReference;
            IsValid = videoManifest.IsValid;
            ValidationTime = videoManifest.ValidationTime;
            VideoId = video.Id;
        }

        // Properties.
        public IEnumerable<ErrorDetailDto> ErrorDetails { get; private set; }
        public SwarmReference Hash { get; private set; }
        public bool? IsValid { get; private set; }
        public DateTime? ValidationTime { get; private set; }
        public string VideoId { get; private set; }
    }
}
