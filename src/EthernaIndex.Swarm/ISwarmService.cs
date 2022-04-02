using Etherna.EthernaIndex.Swarm.DtoModel;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Swarm
{
    public interface ISwarmService
    {
        Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash);

#if DEBUG_MOCKUP_SWARM
        string GenerateNewHash();
        void SetupHashMockup(string hash, object returnedObject);
        MetadataVideoDto SetupNewMetadataVideoMockup(string manifestHash);
#endif
    }
}
