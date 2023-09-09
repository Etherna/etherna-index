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
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoManifestStatusDto
    {
        // Constructors.
        public VideoManifestStatusDto(VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            ErrorDetails = videoManifest.ValidationErrors
                .Select(i => new ErrorDetailDto(i.ErrorMessage, i.ErrorType));
            Hash = videoManifest.Manifest.Hash;
            IsValid = videoManifest.IsValid;
            ValidationTime = videoManifest.ValidationTime;
        }

        // Properties.
        public IEnumerable<ErrorDetailDto> ErrorDetails { get; private set; }
        public string Hash { get; private set; }
        public bool? IsValid { get; private set; }
        public DateTime? ValidationTime { get; private set; }
    }
}
