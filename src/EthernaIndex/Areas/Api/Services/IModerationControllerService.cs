using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IModerationControllerService
    {
        Task ModerateCommentAsync(string id);
        Task ModerateVideoAsync(string id);
    }
}