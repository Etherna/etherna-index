﻿//   Copyright 2021-present Etherna Sagl
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

using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IUsersControllerService
    {
        Task<UserDto> FindByAddressAsync(string address);
        Task<CurrentUserDto> GetCurrentUserAsync();
        Task<PaginatedEnumerableDto<UserDto>> GetUsersAsync(int page, int take);
        Task<PaginatedEnumerableDto<Video2Dto>> GetVideosAsync(string address, int page, int take);

        //deprecated
        [Obsolete("Used only for API backwards compatibility")]
        Task<PaginatedEnumerableDto<VideoDto>> GetVideosAsync_old(string address, int page, int take);
    }
}