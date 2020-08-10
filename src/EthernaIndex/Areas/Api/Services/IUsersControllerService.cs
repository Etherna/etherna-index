using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IUsersControllerService
    {
        Task<UserDto> FindByAddressAsync(string address);
        Task<UserPrivateDto> GetCurrentUserAsync();
        Task<IEnumerable<UserDto>> GetUsersAsync(bool onlyWithVideo, int page, int take);
        Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take);
        Task UpdateCurrentUserIdentityManifestAsync(string? hash);
    }
}