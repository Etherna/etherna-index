using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
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
        public ChannelsControllerService(
            IIndexContext indexContext)
        {
            this.indexContext = indexContext;
        }

        // Methods.
        public async Task<UserDto> FindByAddressAsync(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            address = address.ConvertToEthereumChecksumAddress();

            return new UserDto(await indexContext.Users.FindOneAsync(c => c.Address == address));
        }

        public async Task<IEnumerable<UserDto>> GetChannelsAsync(int page, int take) =>
            (await indexContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Videos.Any())
                        .PaginateDescending(u => u.CreationDateTime, page, take)
                        .ToListAsync()))
            .Select(c => new UserDto(c));

        public async Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take)
        {
            var user = await indexContext.Users.FindOneAsync(c => c.Address == address);
            return user.Videos.PaginateDescending(v => v.CreationDateTime, page, take)
                                 .Select(v => new VideoDto(v));
        }
    }
}
