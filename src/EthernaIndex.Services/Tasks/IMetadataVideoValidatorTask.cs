using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public interface IMetadataVideoValidatorTask
    {
        Task RunAsync(string videoId, string manifestHash, bool forceNewValidation);
    }
}
