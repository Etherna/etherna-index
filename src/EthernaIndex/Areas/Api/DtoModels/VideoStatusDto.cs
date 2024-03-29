﻿//   Copyright 2021-present Etherna Sagl
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
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            Id = video.Id;
            ManifestsStatus = video.VideoManifests.Select(vm => new VideoManifestStatusDto(video, vm));
        }

        // Properties.
        public string Id { get; private set; }
        public IEnumerable<VideoManifestStatusDto> ManifestsStatus { get; private set; }
    }
}
