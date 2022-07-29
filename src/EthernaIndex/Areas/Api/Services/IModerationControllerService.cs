using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IModerationControllerService
    {
        Task RemoveVideoAsync(string id);
    }
}