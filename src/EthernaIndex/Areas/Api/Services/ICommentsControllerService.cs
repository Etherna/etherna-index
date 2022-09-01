using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface ICommentsControllerService
    {
        Task DeleteOwnedCommentAsync(string id, ClaimsPrincipal currentUserClaims);
    }
}