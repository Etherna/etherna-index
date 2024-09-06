// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Asp.Versioning;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.Services;
using Etherna.EthernaIndex.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class UsersController(IUsersControllerService service) : ControllerBase
    {
        // Get.

        /// <summary>
        /// Get a complete list of users.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet]
        [Obsolete("Use \"list2\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<UserDto>> GetUsersAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            (await service.GetUsersAsync(page, take)).Elements;

        /// <summary>
        /// Get a complete list of users.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("list2")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<PaginatedEnumerableDto<UserDto>> GetUsers2Async(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetUsersAsync(page, take);

        /// <summary>
        /// Get user info by address.
        /// </summary>
        /// <param name="address">The user ether address</param>
        [HttpGet("{address}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<UserDto> FindByAddressAsync(
            string address) =>
            service.FindByAddressAsync(address);

        /// <summary>
        /// Get list of videos uploaded by an user.
        /// </summary>
        /// <param name="address">Address of user</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">List of user's videos</response>
        /// <response code="404">User not found</response>
        [HttpGet("{address}/videos")]
        [Obsolete("Use \"videos3\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<VideoDto>> GetVideosAsync_old0(
            [Required] string address,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            (await service.GetVideosAsync_old(address, page, take)).Elements;

        /// <summary>
        /// Get list of videos uploaded by an user.
        /// </summary>
        /// <param name="address">Address of user</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">List of user's videos</response>
        /// <response code="404">User not found</response>
        [HttpGet("{address}/videos2")]
        [Obsolete("Use \"videos3\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<PaginatedEnumerableDto<VideoDto>> GetVideosAsync_old1(
            [Required] string address,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideosAsync_old(address, page, take);

        /// <summary>
        /// Get list of videos uploaded by an user.
        /// </summary>
        /// <param name="address">Address of user</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">List of user's videos</response>
        /// <response code="404">User not found</response>
        [HttpGet("{address}/videos3")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<PaginatedEnumerableDto<Video2Dto>> GetVideosAsync(
            [Required] string address,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideosAsync(address, page, take);

        [HttpGet("current")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<CurrentUserDto> GetCurrentUserAsync() =>
            service.GetCurrentUserAsync();
    }
}
