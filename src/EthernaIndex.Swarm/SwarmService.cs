using Etherna.BeeNet;
using EthernaIndex.Swarm.DtoModel;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EthernaIndex.Swarm
{
    public class SwarmService : ISwarmService
    {
        // Fields.
        private readonly IBeeNodeClient BeeNodeClient;
        private readonly SwarmSettings SwarmSettings;

        // Constructors.
        public SwarmService(IOptions<SwarmSettings> swarmSettings)
        {
            SwarmSettings = swarmSettings.Value;
            BeeNodeClient = new BeeNodeClient(SwarmSettings.GatewayUrl);
        }

        // Methods.
        public async Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash)
        {
            using var stream = await BeeNodeClient.GatewayClient.BytesGetAsync(manifestHash);
            using var reader = new StreamReader(stream);
            try
            {
                return JsonSerializer.Deserialize<MetadataVideoDto>(reader.ReadToEnd());
            }
            catch(Exception ex)
            {
                throw new MetadataVideoException(ex);
            }
        }
    }
}
