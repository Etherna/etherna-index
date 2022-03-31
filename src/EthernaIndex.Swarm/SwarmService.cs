using Etherna.BeeNet;
using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.EthernaIndex.Swarm.DtoModel;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
        private readonly System.Collections.Generic.Dictionary<string, object> SwarmObjectMockups = new(); //hash->object
#endif

        // Constructors.
        public SwarmService(IOptions<SwarmSettings> swarmSettings)
        {
            if (swarmSettings?.Value is null)
                throw new ArgumentNullException(nameof(swarmSettings));

            SwarmSettings = swarmSettings.Value;
            var gatewayApiVersion = SwarmSettings.GatewayApiVersion switch
            {
                "2.0.0" => GatewayApiVersion.v2_0_0,
                _ => throw new ArgumentOutOfRangeException(nameof(SwarmService), "Invalid gateway api version") 
            };

            BeeNodeClient = new BeeNodeClient(
                SwarmSettings.NodeUrl,
                gatewayApiVersion: gatewayApiVersion);
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
        public void SetupHashMockup(string hash, object returnedObject) =>
            SwarmObjectMockups[hash] = returnedObject;
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
