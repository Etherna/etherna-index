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

#if DEBUG_MOCKUP_SWARM
using System;
using System.Collections.Generic;
using Etherna.BeeNet.Models;
using Etherna.Sdk.Tools.Video.Models;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Infrastructure
{
    public class SwarmService : ISwarmService
    {
        // Fields.
        private readonly Dictionary<SwarmHash, object> SwarmObjectMockups = new(); //hash->object
        
        // Methods.
        public Task<PublishedVideoManifest> GetPublishedVideoManifestAsync(SwarmHash manifestHash) =>
            Task.FromResult((PublishedVideoManifest)SwarmObjectMockups[manifestHash]);

        public void SetupHashMockup(SwarmHash hash, object returnedObject) =>
            SwarmObjectMockups[hash] = returnedObject;

        public PublishedVideoManifest SetupNewPublishedVideoManifestMockup(SwarmHash manifestHash)
        {
            var manifest = new PublishedVideoManifest(
                manifestHash,
                new VideoManifest(
                    1.77f,
                    PostageBatchId.Zero,
                    DateTimeOffset.UtcNow,
                    "Test description",
                    TimeSpan.FromMinutes(10),
                    "Mocked sample video",
                    Nethereum.Util.AddressUtil.ZERO_ADDRESS,
                    """{"test":"sample"}""",
                    [new VideoManifestVideoSource("sources/playlist.m3u8", VideoType.Hls, null, 100000000, [], SwarmHash.Zero)],
                    new VideoManifestImage(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", [new VideoManifestImageSource("myThumb.jpg", ImageType.Jpeg, 480, SwarmHash.Zero)]),
                    []
                ),
                []);

            SetupHashMockup(manifestHash, manifest);

            return manifest;
        }
    }
}
#else
using Etherna.Sdk.Tools.Video.Services;
using Etherna.BeeNet.Models;
using Etherna.Sdk.Tools.Video.Models;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Infrastructure
{
    public class SwarmService(IVideoManifestService videoManifestService) : ISwarmService
    {
        // Methods.
        public Task<PublishedVideoManifest> GetPublishedVideoManifestAsync(SwarmHash manifestHash) =>
            videoManifestService.GetPublishedVideoManifestAsync(manifestHash);
    }
}
#endif
