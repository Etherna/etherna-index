using Etherna.BeeNet;
using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.EthernaIndex.Swarm.DtoModel;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Swarm
{
    public class SwarmService : ISwarmService
    {
        // Fields.
        private readonly IBeeNodeClient BeeNodeClient;
        private readonly SwarmSettings SwarmSettings;

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

            using var stream = await BeeNodeClient.GatewayClient.GetFileAsync(manifestHash);
            using var reader = new StreamReader(stream);
            try
            {
                var metadataVideoDto = JsonSerializer.Deserialize<MetadataVideoDto>(await reader.ReadToEndAsync(), _jsonDeserializeOptions);
                if (metadataVideoDto is null)
                    throw new MetadataVideoException("Empty json");

                return metadataVideoDto;
            }
            catch(Exception ex)
            {
                throw new MetadataVideoException("Unable to cast json", ex);
            }
        }

        // Helpers.
        private static readonly JsonSerializerOptions _jsonDeserializeOptions = new() { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } };

    }
}
