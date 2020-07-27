﻿using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IChannelsControllerService
    {
        Task<VideoDto> AddVideoAsync(string address, VideoCreateInput videoInput);
        Task<ChannelDto> CreateAsync(ChannelCreateInput channelInput);
        Task<IEnumerable<ChannelDto>> GetChannelsAsync(int page, int take);
        Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take);
        Task<ActionResult> RemoveVideoAsync(string address, string videoHash);
        Task<ChannelDto> UpdateAsync(ChannelCreateInput channelInput);
        Task<ChannelDto> FindByAddressAsync(string address);
    }
}