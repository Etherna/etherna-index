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
using Etherna.BeeNet.Stores;
using Etherna.Sdk.Tools.Video.Models;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Infrastructure
{
    public class SwarmService : ISwarmService
    {
        // Fields.
        private readonly Dictionary<SwarmReference, object> SwarmObjectMockups = new();

        // Methods.
        public Task<PublishedVideoManifest> GetPublishedVideoManifestAsync(
            SwarmReference manifestReference,
            IReadOnlyChunkStore chunkStore) =>
            Task.FromResult((PublishedVideoManifest)SwarmObjectMockups[manifestReference]);

        public void SetupHashMockup(SwarmReference reference, object returnedObject) =>
            SwarmObjectMockups[reference] = returnedObject;

        public PublishedVideoManifest SetupNewPublishedVideoManifestMockup(SwarmReference manifestReference)
        {
            var manifest = new PublishedVideoManifest(
                manifestReference,
                new VideoManifest(
                    1.77f,
                    PostageBatchId.Zero,
                    DateTimeOffset.UtcNow,
                    "Test description",
                    TimeSpan.FromMinutes(10),
                    "Mocked sample video",
                    Nethereum.Util.AddressUtil.ZERO_ADDRESS,
                    """{"test":"sample"}""",
                    [new VideoManifestVideoSource("sources/playlist.m3u8", VideoType.Hls, null, 100000000, [], SwarmReference.PlainZero)],
                    new VideoManifestImage(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", [new VideoManifestImageSource("myThumb.jpg", ImageType.Jpeg, 480, SwarmReference.PlainZero)]),
                    []
                ),
                []);

            SetupHashMockup(manifestReference, manifest);

            return manifest;
        }
    }
}
#else
using Etherna.Sdk.Tools.Video.Services;
using Etherna.BeeNet.Models;
using Etherna.BeeNet.Stores;
using Etherna.Sdk.Tools.Video.Models;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Infrastructure
{
    public class SwarmService(IVideoManifestService videoManifestService) : ISwarmService
    {
        // Methods.
        public Task<PublishedVideoManifest> GetPublishedVideoManifestAsync(
            SwarmReference manifestReference,
            IReadOnlyChunkStore chunkStore) =>
            videoManifestService.GetPublishedVideoManifestAsync(manifestReference, chunkStore);
    }
}
#endif
