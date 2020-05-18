using Digicando.MongODM.Extensions;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
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
        public async Task<VideoDto> AddVideoAsync(string address, VideoCreateInput videoInput)
        {
            var channel = await indexContext.Channels.FindOneAsync(c => c.Address == address);
            var video = new Video(
                videoInput.Description,
                TimeSpan.FromSeconds(videoInput.LengthInSeconds),
                channel,
                videoInput.ThumbnailHash,
                videoInput.Title,
                videoInput.VideoHash);

            await indexContext.Videos.CreateAsync(video);

            return new VideoDto(video);
        }

        public async Task<ChannelDto> CreateAsync(ChannelCreateInput channelInput)
        {
            var channel = new Channel(channelInput.Address);
            await indexContext.Channels.CreateAsync(channel);
            return new ChannelDto(channel);
        }

        public async Task<ChannelDto> FindByAddressAsync(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            address = address.ConvertToEthereumChecksumAddress();

            return new ChannelDto(await indexContext.Channels.FindOneAsync(c => c.Address == address));
        }

        public async Task<IEnumerable<ChannelDto>> GetChannelsAsync(int page, int take) =>
            (await indexContext.Channels.QueryElementsAsync(elements =>
                elements.PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync()))
            .Select(c => new ChannelDto(c));

        public async Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take)
        {
            var channel = await indexContext.Channels.FindOneAsync(c => c.Address == address);
            return channel.Videos.PaginateDescending(v => v.CreationDateTime, page, take)
                                 .Select(v => new VideoDto(v));
        }

        public async Task<ActionResult> RemoveVideoAsync(string address, string videoHash)
        {
            var channel = await indexContext.Channels.FindOneAsync(c => c.Address == address);
            var video = channel.Videos.First(v => v.VideoHash == videoHash);

            await indexContext.Videos.DeleteAsync(video);

            return new OkResult();
        }

        public async Task<ChannelDto> UpdateAsync(ChannelCreateInput channelInput)
        {
            var channel = await indexContext.Channels.FindOneAsync(c => c.Address == channelInput.Address);

            await indexContext.SaveChangesAsync();

            return new ChannelDto(channel);
        }
    }
}
