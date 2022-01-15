using Etherna.BeeNet;
using Etherna.BeeNet.DtoModel;
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
            if (Enum.TryParse($"v{SwarmSettings.Version.Replace(".", "_")}", out ClientVersions ClientVersion))
            {
                ClientVersion = ClientVersions.v1_4_1;
            }
            BeeNodeClient = new BeeNodeClient(SwarmSettings.GatewayUrl, version: ClientVersion);
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
