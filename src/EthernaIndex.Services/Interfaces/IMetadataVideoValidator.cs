using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Interfaces
{
    public interface IMetadataVideoValidator
    {
        Task<bool> CheckManifestAsync(string manifestHash);
    }
}
