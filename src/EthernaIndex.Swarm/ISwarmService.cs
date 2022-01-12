using EthernaIndex.Swarm.DtoModel;
using System.Threading.Tasks;

namespace EthernaIndex.Swarm
{
    public interface ISwarmService
    {
        Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash);
    }
}
