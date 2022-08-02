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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    class UserService : IUserService
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;
        private readonly ISharedDbContext sharedDbContext;

        // Constructor.
        public UserService(
            IIndexDbContext indexDbContext,
            ISharedDbContext sharedDbContext)
        {
            this.indexDbContext = indexDbContext;
            this.sharedDbContext = sharedDbContext;
        }

        // Methods.
        public async Task<(User, UserSharedInfo)> FindUserAsync(string address) =>
            await FindUserAsync(await FindUserSharedInfoByAddressAsync(address));

        public async Task<(User, UserSharedInfo)> FindUserAsync(UserSharedInfo userSharedInfo)
        {
            // Try find user.
            var user = await indexDbContext.Users.TryFindOneAsync(u => u.SharedInfoId == userSharedInfo.Id);

            // If user doesn't exist.
            if (user is null)
            {
                // Create a new user.
                user = new User(userSharedInfo);
                await indexDbContext.Users.CreateAsync(user);

                // Get again, because of https://etherna.atlassian.net/browse/MODM-83
                user = await indexDbContext.Users.FindOneAsync(user.Id);
            }

            return (user, userSharedInfo);
        }

        public async Task<UserSharedInfo> FindUserSharedInfoByAddressAsync(string address)
        {
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid ethereum address", nameof(address));

            // Normalize address.
            address = address.ConvertToEthereumChecksumAddress();

            // Find user shared info.
            return await sharedDbContext.UsersInfo.QueryElementsAsync(elements =>
                elements.Where(u => u.EtherAddress == address ||                   //case: db and invoker are synced
                                    u.EtherPreviousAddresses.Contains(address))    //case: db is ahead than invoker
                        .FirstAsync());
        }

        public async Task<(User?, UserSharedInfo?)> TryFindUserAsync(string address)
        {
            var sharedInfo = await TryFindUserSharedInfoByAddressAsync(address);
            if (sharedInfo is null)
                return (null, null);

            return await FindUserAsync(sharedInfo);
        }

        public async Task<UserSharedInfo?> TryFindUserSharedInfoByAddressAsync(string address)
        {
            try { return await FindUserSharedInfoByAddressAsync(address); }
            catch (InvalidOperationException) { return null; }
        }
    }
}
