using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.EthernaIndex.Extensions;
using Etherna.MongODM.Extensions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class UsersControllerService : IUsersControllerService
    {
        // Fields.
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIndexContext indexContext;

        // Constructors.
        public UsersControllerService(
            IHttpContextAccessor httpContextAccessor,
            IIndexContext indexContext)
        {
            this.httpContextAccessor = httpContextAccessor;
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

        public async Task<UserPrivateDto> GetCurrentUserAsync()
        {
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var prevAddresses = httpContextAccessor.HttpContext.User.GetEtherPrevAddresses();

            var manifest = await indexContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address ||
                                    prevAddresses.Contains(u.Address))
                        .Select(u => u.IdentityManifest)
                        .FirstAsync());

            return new UserPrivateDto(address, manifest?.Hash, prevAddresses);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(
            bool onlyWithVideo, int page, int take) =>
            (await indexContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => !onlyWithVideo || u.Videos.Any())
                        .PaginateDescending(u => u.CreationDateTime, page, take)
                        .ToListAsync()))
              .Select(c => new UserDto(c));

        public async Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take)
        {
            var user = await indexContext.Users.FindOneAsync(c => c.Address == address);
            return user.Videos.PaginateDescending(v => v.CreationDateTime, page, take)
                                 .Select(v => new VideoDto(v));
        }

        public async Task UpdateCurrentUserIdentityManifestAsync(string? hash)
        {
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var prevAddresses = httpContextAccessor.HttpContext.User.GetEtherPrevAddresses();

            var user = await indexContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address ||
                                    prevAddresses.Contains(u.Address))
                        .FirstAsync());

            user.IdentityManifest = hash is null ?
                null :
                new SwarmContentHash(hash);

            await indexContext.SaveChangesAsync();
        }
    }
}
