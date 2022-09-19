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

using Etherna.Authentication;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Extensions;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IAuthorizationService authorizationService;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIndexDbContext indexDbContext;
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;

        // Constructors.
        public UsersControllerService(
            IAuthorizationService authorizationService,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IHttpContextAccessor httpContextAccessor,
            IIndexDbContext indexDbContext,
            ISharedDbContext sharedDbContext,
            IUserService userService)
        {
            this.authorizationService = authorizationService;
            this.ethernaOidcClient = ethernaOidcClient;
            this.httpContextAccessor = httpContextAccessor;
            this.indexDbContext = indexDbContext;
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

            return new UserDto(user, sharedInfo);
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync()
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, sharedInfo) = await userService.FindUserAsync(address);

            var isSuperModeratorResult = await authorizationService.AuthorizeAsync(
                httpContextAccessor.HttpContext!.User, CommonConsts.RequireSuperModeratorClaimPolicy);

            return new CurrentUserDto(user, sharedInfo, isSuperModeratorResult.Succeeded);
        }

        public async Task<PaginatedEnumerableDto<UserDto>> GetUsersAsync(
            bool onlyWithVideo, int page, int take)
        {
            var paginatedUsers = await indexDbContext.Users.QueryPaginatedElementsAsync(
                elements => elements.Where(u => !onlyWithVideo || u.Videos.Any(v => v.LastValidManifest != null)),
                u => u.CreationDateTime,
                page,
                take,
                true);

            var userDtos = new List<UserDto>();
            foreach (var user in paginatedUsers.Elements)
            {
                var sharedInfo = await sharedDbContext.UsersInfo.TryFindOneAsync(user.SharedInfoId);
                userDtos.Add(new UserDto(user, sharedInfo));
            }

            return new PaginatedEnumerableDto<UserDto>(
                paginatedUsers.CurrentPage,
                userDtos,
                paginatedUsers.PageSize,
                paginatedUsers.TotalElements);
        }

        public async Task<PaginatedEnumerableDto<VideoDto>> GetVideosAsync(string address, int page, int take)
        {
            var currentUserAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            var requestByVideoOwner = address == currentUserAddress;

            var (user, sharedInfo) = await userService.FindUserAsync(address);

            return new PaginatedEnumerableDto<VideoDto>(
                page,
                user.Videos.Where(v => requestByVideoOwner || v.LastValidManifest != null)
                           .PaginateDescending(v => v.CreationDateTime, page, take)
                           .Select(v => new VideoDto(v, v.LastValidManifest, sharedInfo, null)),
                take,
                user.Videos.Where(v => requestByVideoOwner || v.LastValidManifest != null).Count());
        }
    }
}
