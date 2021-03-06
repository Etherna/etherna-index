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
using Etherna.EthernaIndex.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.2")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class SystemController : ControllerBase
    {
        // Fields.
        private readonly ISystemControllerService controllerService;

        // Constructors.
        public SystemController(ISystemControllerService controllerService)
        {
            this.controllerService = controllerService;
        }

        // Get.

        /// <summary>
        /// Get current index settings.
        /// </summary>
        [HttpGet("settings")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<SettingsDto> GetSettingsAsync() =>
            controllerService.GetSettingsAsync();

        // Post.

        /// <summary>
        /// Migrate index db.
        /// </summary>
        /// <returns></returns>
        [HttpPost("migratedb")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IActionResult> MigrateDatabase() =>
            controllerService.MigrateDatabaseAsync(User.GetEtherAddress());
    }
}
