using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Video
{
    public interface IVideoService
    {
        Task ValidateMetadataAsync(string manifestHash);
    }
}
