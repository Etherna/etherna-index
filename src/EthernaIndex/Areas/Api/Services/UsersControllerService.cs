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
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal sealed class UsersControllerService : IUsersControllerService
    {
        // Fields.
        private readonly IAuthorizationService authorizationService;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIndexDbContext indexDbContext;
        private readonly ILogger<UsersControllerService> logger;
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;

        // Constructors.
        public UsersControllerService(
            IAuthorizationService authorizationService,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IHttpContextAccessor httpContextAccessor,
            IIndexDbContext indexDbContext,
            ILogger<UsersControllerService> logger,
            ISharedDbContext sharedDbContext,
            IUserService userService)
        {
            this.authorizationService = authorizationService;
            this.ethernaOidcClient = ethernaOidcClient;
            this.httpContextAccessor = httpContextAccessor;
            this.indexDbContext = indexDbContext;
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

            logger.FindUserByAddress(address);

            return new UserDto(user, sharedInfo);
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync()
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, sharedInfo) = await userService.FindUserAsync(address);

            var isSuperModeratorResult = await authorizationService.AuthorizeAsync(
                httpContextAccessor.HttpContext!.User, CommonConsts.RequireSuperModeratorClaimPolicy);

            logger.GetCurrentUser(address);

            return new CurrentUserDto(user, sharedInfo, isSuperModeratorResult.Succeeded);
        }

        public async Task<PaginatedEnumerableDto<UserDto>> GetUsersAsync(int page, int take)
        {
            var paginatedUsers = await indexDbContext.Users.QueryPaginatedElementsAsync(
                elements => elements,
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

            logger.GetUserListPaginated(page, take);

            return new PaginatedEnumerableDto<UserDto>(
                paginatedUsers.CurrentPage,
                userDtos,
                paginatedUsers.PageSize,
                paginatedUsers.TotalElements);
        }

        public async Task<PaginatedEnumerableDto<Video2Dto>> GetVideosAsync(string address, int page, int take)
        {
            var currentUserAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            var requestByVideoOwner = address == currentUserAddress;

            var (user, sharedInfo) = await userService.FindUserAsync(address);
            var paginatedVideos = await indexDbContext.Videos.QueryPaginatedElementsAsync(
                elements => elements.Where(v => v.Owner.Id == user.Id)
                                    .Where(v => requestByVideoOwner || v.LastValidManifest != null),
                v => v.CreationDateTime,
                page,
                take,
                true);

            logger.GetUserVideosPaginated(address, page, take);

            return new PaginatedEnumerableDto<Video2Dto>(
                paginatedVideos.CurrentPage,
                paginatedVideos.Elements.Select(v => new Video2Dto(v, v.LastValidManifest, sharedInfo, null)),
                paginatedVideos.PageSize,
                paginatedVideos.TotalElements);
        }

        //deprecated

        [Obsolete("Used only for API backwards compatibility")]
        public async Task<PaginatedEnumerableDto<VideoDto>> GetVideosAsync_old(string address, int page, int take)
        {
            var currentUserAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            var requestByVideoOwner = address == currentUserAddress;

            var (user, sharedInfo) = await userService.FindUserAsync(address);
            var paginatedVideos = await indexDbContext.Videos.QueryPaginatedElementsAsync(
                elements => elements.Where(v => v.Owner.Id == user.Id)
                                    .Where(v => requestByVideoOwner || v.LastValidManifest != null),
                v => v.CreationDateTime,
                page,
                take,
                true);

            logger.GetUserVideosPaginated(address, page, take);

            return new PaginatedEnumerableDto<VideoDto>(
                paginatedVideos.CurrentPage,
                paginatedVideos.Elements.Select(v => new VideoDto(v, v.LastValidManifest, sharedInfo, null)),
                paginatedVideos.PageSize,
                paginatedVideos.TotalElements);
        }
    }
}
