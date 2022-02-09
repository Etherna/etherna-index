using Etherna.EthernaIndex.Swarm.DtoModel;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Swarm
{
    public interface ISwarmService
    {
        Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash);
    }
}
