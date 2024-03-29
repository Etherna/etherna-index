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
    public class UsersController : ControllerBase
    {
        // Fields.
        private readonly IUsersControllerService controllerService;

        // Constructors.
        public UsersController(IUsersControllerService controllerService)
        {
            this.controllerService = controllerService;
        }

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
            (await controllerService.GetUsersAsync(page, take)).Elements;

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
            controllerService.GetUsersAsync(page, take);

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
            controllerService.FindByAddressAsync(address);

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
            (await controllerService.GetVideosAsync_old(address, page, take)).Elements;

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
            controllerService.GetVideosAsync_old(address, page, take);

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
            controllerService.GetVideosAsync(address, page, take);

        [HttpGet("current")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<CurrentUserDto> GetCurrentUserAsync() =>
            controllerService.GetCurrentUserAsync();
    }
}
