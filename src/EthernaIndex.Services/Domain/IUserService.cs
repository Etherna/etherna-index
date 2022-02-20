using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    public interface IUserService
    {
        Task<(User, UserSharedInfo)> FindUserAsync(string address);
        Task<(User, UserSharedInfo)> FindUserAsync(UserSharedInfo userSharedInfo);
        Task<UserSharedInfo> FindUserSharedInfoByAddressAsync(string address);
        Task<(User?, UserSharedInfo?)> TryFindUserAsync(string address);
        Task<UserSharedInfo?> TryFindUserSharedInfoByAddressAsync(string address);
    }
}
