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

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels.ManifestV1
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class VideoManifestV1Dto
    {
        // Properties.
        //from v1.0
        public string Description { get; set; }
        public long Duration { get; set; }
        public string OriginalQuality { get; set; }
        public string OwnerAddress { get; set; }
        public IEnumerable<VideoSourceV1Dto> Sources { get; set; }
        public ThumbnailV1Dto? Thumbnail { get; set; }
        public string Title { get; set; }
        public string V { get; set; }

        //from v1.1
        public string? BatchId { get; set; }
        public long? CreatedAt { get; set; }
        public long? UpdatedAt { get; set; }

        //from v1.2
        public string? PersonalData { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
