using Digicando.MongODM.Extensions;
using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<VideoDto> AddVideoAsync(string address, VideoInput videoInput)
        {
            var channel = await indexContext.Channels.QueryElementsAsync(elements =>
                elements.Where(c => c.Address == address)
                        .FirstAsync());
            var video = new Video(
                videoInput.Description,
                videoInput.ThumbnailHash,
                videoInput.Title,
                videoInput.VideoHash);

            channel.AddVideo(video);

            await indexContext.SaveChangesAsync();

            return new VideoDto(video);
        }

        public async Task<ChannelDto> CreateAsync(ChannelInput channelInput)
        {
            var channel = new Channel(channelInput.Address, channelInput.BannerHash);
            await indexContext.Channels.CreateAsync(channel);
            return new ChannelDto(channel);
        }

        public async Task<ChannelDto> FindByAddressAsync(string address) =>
            new ChannelDto(await indexContext.Channels.QueryElementsAsync(elements =>
                elements.Where(c => c.Address == address)
                        .FirstAsync()));

        public async Task<IEnumerable<ChannelDto>> GetChannelsAsync(int page, int take) =>
            (await indexContext.Channels.QueryElementsAsync(elements =>
                elements.PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync()))
            .Select(c => new ChannelDto(c));

        public async Task<IEnumerable<VideoDto>> GetVideosAsync(string address)
        {
            var channel = await indexContext.Channels.QueryElementsAsync(elements =>
                elements.Where(c => c.Address == address)
                        .FirstAsync());
            return channel.Videos.Select(v => new VideoDto(v));
        }

        public async Task<ActionResult> RemoveVideoAsync(string address, string videoHash)
        {
            var channel = await indexContext.Channels.QueryElementsAsync(elements =>
                elements.Where(c => c.Address == address)
                        .FirstAsync());
            var video = channel.Videos.First(v => v.VideoHash == videoHash);

            channel.RemoveVideo(video);

            return new OkResult();
        }

        public async Task<ChannelDto> UpdateAsync(ChannelInput channelInput)
        {
            var channel = await indexContext.Channels.QueryElementsAsync(elements =>
                elements.Where(c => c.Address == channelInput.Address)
                        .FirstAsync());

            channel.SetBannerHash(channelInput.BannerHash);

            await indexContext.SaveChangesAsync();

            return new ChannelDto(channel);
        }
    }
}
