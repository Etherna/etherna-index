using Etherna.BeeNet;
using Etherna.BeeNet.DtoModel;
using Etherna.EthernaIndex.Swarm.DtoModel;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
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
            var clientVersion = SwarmSettings.Version switch
            {
                _ => ClientVersions.v1_4_1,
            };

            BeeNodeClient = new BeeNodeClient(SwarmSettings.GatewayUrl, version: clientVersion);
        }

        // Methods.
        public async Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash)
        {
            if (BeeNodeClient.GatewayClient is null)
                throw new InvalidOperationException(nameof(BeeNodeClient.GatewayClient));

            using var stream = await BeeNodeClient.GatewayClient.BytesGetAsync(manifestHash);
            using var reader = new StreamReader(stream);
            try
            {
                var metadataVideoDto = JsonSerializer.Deserialize<MetadataVideoDto>(reader.ReadToEnd());
                if (metadataVideoDto is null)
                    throw new MetadataVideoException("Empty json");

                return metadataVideoDto;
            }
            catch(Exception ex)
            {
                throw new MetadataVideoException("Unable to cast json", ex);
            }
        }
    }
}
