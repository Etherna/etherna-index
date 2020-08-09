using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IChannelsControllerService
    {
        Task<IEnumerable<UserDto>> GetChannelsAsync(int page, int take);
        Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take);
        Task<UserDto> FindByAddressAsync(string address);
    }
}