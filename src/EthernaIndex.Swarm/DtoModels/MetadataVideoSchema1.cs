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

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MetadataVideoSchema1
    {
        // Properties.
        public string Description { get; set; }
        public long Duration { get; set; }
        public long CreatedAt { get; set; }
        public string OriginalQuality { get; set; }
        public string OwnerAddress { get; set; }
        public IEnumerable<MetadataVideoSourceSchema1> Sources { get; set; }
        public SwarmImageRawSchema1? Thumbnail { get; set; }
        public string Title { get; set; }
        public string V { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
