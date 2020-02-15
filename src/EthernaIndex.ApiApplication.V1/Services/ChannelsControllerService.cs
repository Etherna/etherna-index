using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Etherna.EthernaIndex.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    internal class ChannelsControllerService : IChannelsControllerService
    {
        // Fields.
        private readonly IIndexContext indexContext;

        // Constructors.
        public ChannelsControllerService(IIndexContext indexContext)
        {
            this.indexContext = indexContext;
        }

        // Methods.
        public Task<ActionResult> AddVideoAsync(string address, VideoInput videoInput)
        {
            throw new NotImplementedException();
        }

        public Task<ChannelDto> CreateAsync(ChannelInput channelInput)
        {
            throw new NotImplementedException();
        }

        public Task<ChannelDto> FindByAddressAsync(string address)
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

        public Task<ChannelDto> UpdateAsync(string address, ChannelInput channelInput)
        {
            throw new NotImplementedException();
        }
    }
}
