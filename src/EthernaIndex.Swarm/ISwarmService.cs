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
using System.Text.Json;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Swarm
{
    public interface ISwarmService
    {
        Task<VideoManifestMetadataBase> DeserializeVideoMetadataAsync(string manifestHash, JsonElement jsonElementManifest);
        Task<VideoManifestMetadataBase> GetVideoMetadataAsync(string manifestHash);
        Task<bool> IsImageAsync(string hash);

#if DEBUG_MOCKUP_SWARM
        string GenerateNewHash();
        void SetupHashMockup(string hash, object returnedObject);
        VideoManifestMetadataBase SetupNewMetadataV1VideoMockup(string manifestHash);
        VideoManifestMetadataBase SetupNewMetadataV2VideoMockup(string manifestHash);
#endif
    }
}
