using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    public interface IChannelsControllerService
    {
        Task<ActionResult> AddVideoAsync(string address, VideoInput videoInput);
        Task<ChannelDto> CreateAsync(ChannelInput channelInput);
        Task<IEnumerable<ChannelDto>> GetChannelsAsync(int page, int take);
        Task<IEnumerable<VideoDto>> GetVideosAsync(string address);
        Task<ActionResult> RemoveVideoAsync(string address, string videoHash);
        Task<ChannelDto> UpdateAsync(string address, ChannelInput channelInput);
        Task<ChannelDto> FindByAddressAsync(string address);
    }
}