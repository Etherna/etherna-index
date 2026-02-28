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

using Etherna.EthernaIndex.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    [Obsolete("Use enumerable of VideoManifestStatusDto instead")]
    public class VideoStatusDto
    {
        // Constructors.
        public VideoStatusDto(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);

            Id = video.Id;
            ManifestsStatus = video.VideoManifests.Select(vm => new VideoManifestStatusDto(video, vm));
        }

        // Properties.
        public string Id { get; private set; }
        public IEnumerable<VideoManifestStatusDto> ManifestsStatus { get; private set; }
    }
}
