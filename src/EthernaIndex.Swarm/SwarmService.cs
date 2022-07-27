using Etherna.BeeNet;
using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.EthernaIndex.Swarm.DtoModel;
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
#pragma warning disable CA1823 // Avoid unused private fields
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
                gatewayApiVersion: GatewayApiVersion.v3_0_1);
        }

        // Methods.
        public async Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash)
        {
            if (BeeNodeClient.GatewayClient is null)
                throw new InvalidOperationException(nameof(BeeNodeClient.GatewayClient));

            try
            {
#if !DEBUG_MOCKUP_SWARM
                using var stream = await BeeNodeClient.GatewayClient.GetFileAsync(manifestHash);
                using var reader = new StreamReader(stream);

                var metadataVideoDto = JsonSerializer.Deserialize<MetadataVideoDto>(await reader.ReadToEndAsync(), _jsonDeserializeOptions);
                if (metadataVideoDto is null)
                    throw new MetadataVideoException("Empty json");

                return metadataVideoDto;
#else
                return (MetadataVideoDto)SwarmObjectMockups[manifestHash];
#endif
            }
            catch(Exception ex)
            {
                throw new MetadataVideoException("Unable to cast json", ex);
            }
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

        public MetadataVideoDto SetupNewMetadataVideoMockup(string manifestHash)
        {
            var manifest = new MetadataVideoDto(
                "Mocked sample video",
                "Test description",
                "720p",
                "0x5E70C176b03BFe5113E78e920C1C60639E2A1696",
                420.0f,
                new SwarmImageRawDto(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string> { { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"} }),
                new[] { new MetadataVideoSourceDto(560000, "720p", GenerateNewHash(), 100000000) });

            SetupHashMockup(manifestHash, manifest);

            return manifest;
        }
#endif

        // Helpers.
        private static readonly JsonSerializerOptions _jsonDeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }
}

#if DEBUG_MOCKUP_SWARM
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore CA1823 // Avoid unused private fields
#endif
