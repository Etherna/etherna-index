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

using Etherna.BeeNet;
using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.EthernaIndex.Swarm.DtoModels;
using Etherna.EthernaIndex.Swarm.Models;
using Etherna.EthernaIndex.Swarm.Exceptions;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#if DEBUG_MOCKUP_SWARM
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
#else
using System.IO;
#endif

#if DEBUG_MOCKUP_SWARM
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#endif

namespace Etherna.EthernaIndex.Swarm
{
    public class SwarmService : ISwarmService
    {
        // Fields.
        private readonly IBeeNodeClient BeeNodeClient;
        private readonly SwarmSettings SwarmSettings;

#if DEBUG_MOCKUP_SWARM
        private readonly Dictionary<string, object> SwarmObjectMockups = new(); //hash->object
        private readonly Random random = new();
#endif

        // Constructors.
        public SwarmService(IOptions<SwarmSettings> swarmSettings)
        {
            if (swarmSettings?.Value is null)
                throw new ArgumentNullException(nameof(swarmSettings));

            SwarmSettings = swarmSettings.Value;
            BeeNodeClient = new BeeNodeClient(
                SwarmSettings.GatewayUrl,
                gatewayApiVersion: GatewayApiVersion.v3_2_0);
        }

        // Methods.
        public MetadataVideo DeserializeMetadataVideo(JsonElement jsonElementManifest)
        {
            // Find version.
            var version = jsonElementManifest.TryGetProperty("v", out var jsonVersion) ?
                jsonVersion.GetString()! :
                "1.0"; //first version didn't have an identifier
            var majorVersion = version.Split('.')[0];

            // Deserialize document.
            var metadataVideoDto = majorVersion switch
            {
                "1" => new MetadataVideo(jsonElementManifest.Deserialize<MetadataVideoSchema1>(
                    new JsonSerializerOptions
                    {
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                        PropertyNameCaseInsensitive = true,
                    }) ?? throw new MetadataVideoException("Empty json")),
                _ => throw new MetadataVideoException("Invalid version")
            };

            return metadataVideoDto;
        }

        public async Task<MetadataVideo> GetMetadataVideoAsync(string manifestHash)
        {
            if (BeeNodeClient.GatewayClient is null)
                throw new InvalidOperationException(nameof(BeeNodeClient.GatewayClient));

#if !DEBUG_MOCKUP_SWARM
            using var manifestStream = await BeeNodeClient.GatewayClient.GetFileAsync(manifestHash);
            var jsonElementManifest = await JsonSerializer.DeserializeAsync<JsonElement>(manifestStream);

            return DeserializeMetadataVideo(jsonElementManifest);
#else
            return (MetadataVideo)SwarmObjectMockups[manifestHash];
#endif
        }

#if DEBUG_MOCKUP_SWARM
        [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Not critical")]
        public string GenerateNewHash()
        {
            var digits = 64;

            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = string.Concat(buffer.Select(x => x.ToString("X2", CultureInfo.InvariantCulture)).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X", CultureInfo.InvariantCulture);
        }

        public void SetupHashMockup(string hash, object returnedObject) =>
            SwarmObjectMockups[hash] = returnedObject;

        public MetadataVideo SetupNewMetadataVideoMockup(string manifestHash)
        {
            var manifest = new MetadataVideo(
                "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                "Test description",
                420,
                DateTime.UtcNow.Ticks,
                "720p",
                "0x5E70C176b03BFe5113E78e920C1C60639E2A1696",
                new[] { new MetadataVideoSource(560000, "720p", GenerateNewHash(), 100000000) },
                new SwarmImageRaw(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string> { { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"} }),
                "Mocked sample video",
                DateTime.UtcNow.Ticks);

            SetupHashMockup(manifestHash, manifest);

            return manifest;
        }
#endif
    }
}

#if DEBUG_MOCKUP_SWARM
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
