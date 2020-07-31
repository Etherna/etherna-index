using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Extensions;
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
        public async Task<ChannelDto> CreateAsync(string address)
        {
            var channel = await indexContext.Channels.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(c => c.Address == address));

            if (channel is null)
            {
                channel = new Channel(address);
                await indexContext.Channels.CreateAsync(channel);
            }

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
    }
}
