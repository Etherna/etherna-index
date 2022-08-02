using Etherna.EthernaIndex.Swarm.Models;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Swarm
{
    public interface ISwarmService
    {
        Task<MetadataVideo> GetMetadataVideoAsync(string manifestHash);

#if DEBUG_MOCKUP_SWARM
        string GenerateNewHash();
        void SetupHashMockup(string hash, object returnedObject);
        MetadataVideoDto SetupNewMetadataVideoMockup(string manifestHash);
#endif
    }
}
