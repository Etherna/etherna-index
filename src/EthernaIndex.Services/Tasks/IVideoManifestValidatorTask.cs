using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public interface IVideoManifestValidatorTask
    {
        Task RunAsync(string videoId, string manifestHash);
    }
}
