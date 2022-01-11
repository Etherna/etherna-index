using Etherna.EthernaIndex.Domain.DtoModel;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Swarm
{
    public interface ISwarmService
    {
        Task<MetadataVideoDto> GetMetadataVideoAsync(string manifestHash);
    }
}
