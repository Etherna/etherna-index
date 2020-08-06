using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IChannelsControllerService
    {
        Task<ChannelDto> CreateAsync();
        Task<IEnumerable<ChannelDto>> GetChannelsAsync(int page, int take);
        Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take);
        Task<ChannelDto> FindByAddressAsync(string address);
    }
}