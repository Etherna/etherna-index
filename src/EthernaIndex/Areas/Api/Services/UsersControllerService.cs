//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.Authentication.Extensions;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Extensions;
using Microsoft.AspNetCore.Http;
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
        private readonly IIndexDbContext indexContext;

        // Constructors.
        public UsersControllerService(
            IHttpContextAccessor httpContextAccessor,
            IIndexDbContext indexContext)
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
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
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
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
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
