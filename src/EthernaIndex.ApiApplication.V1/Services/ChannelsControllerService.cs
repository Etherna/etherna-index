using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    public class ChannelsControllerService : IChannelsControllerService
    {
        public Task<ActionResult> AddVideoAsync(string address, string videoHash)
        {
            throw new NotImplementedException();
        }

        public Task<ChannelDto> CreateAsync(ChannelInput channelInput)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ChannelDto>> GetChannelsAsync(int page, int take)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VideoDto>> GetVideosAsync(string address)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> RemoveVideoAsync(string address, string videoHash)
        {
            throw new NotImplementedException();
        }
    }
}
