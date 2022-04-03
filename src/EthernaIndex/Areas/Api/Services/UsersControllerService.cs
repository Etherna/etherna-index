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
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UsersControllerService> logger;
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;

        // Constructors.
        public UsersControllerService(
            IHttpContextAccessor httpContextAccessor,
            IIndexDbContext indexContext,
            ILogger<UsersControllerService> logger,
            ISharedDbContext sharedDbContext,
            IUserService userService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.indexContext = indexContext;
            this.logger = logger;
            this.sharedDbContext = sharedDbContext;
            this.userService = userService;
        }

        // Methods.
        public async Task<UserDto> FindByAddressAsync(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            var (user, sharedInfo) = await userService.FindUserAsync(address);

            logger.UserFindByAddress(address);

            return new UserDto(user, sharedInfo);
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();

            var (user, sharedInfo) = await userService.FindUserAsync(address);

            logger.UserGetCurrent(address);

            return new UserDto(user, sharedInfo);
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync(
            bool onlyWithVideo, int page, int take)
        {
            var users = await indexContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => !onlyWithVideo || u.Videos.Any(v => v.IsValid))
                        .PaginateDescending(u => u.CreationDateTime, page, take)
                        .ToListAsync());

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var sharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(user.SharedInfoId);
                userDtos.Add(new UserDto(user, sharedInfo));
            }

            logger.UserListPaginated(page, take);

            return userDtos;
        }

        public async Task<IEnumerable<VideoDto>> GetVideosAsync(string address, int page, int take)
        {
            var (user, sharedInfo) = await userService.FindUserAsync(address);

            logger.UserGetVideoPaginated(address, page, take);

            return user.Videos
                .Where(v => v.IsValid)
                .PaginateDescending(v => v.CreationDateTime, page, take)
                .Select(v => new VideoDto(v, v.LastValidManifest, sharedInfo));
        }
    }
}
