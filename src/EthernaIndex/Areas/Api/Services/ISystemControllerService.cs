using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface ISystemControllerService
    {
        Task ForceVideoManifestValidationAsync(string manifestHash);
        Task ForceVideoManifestsValidationAsync(string videoId);
    }
}