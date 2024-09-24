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
using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.Services;
using Etherna.EthernaIndex.Attributes;
using Etherna.EthernaIndex.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class SystemController(ISystemControllerService service) : ControllerBase
    {
        // Get.
        /// <summary>
        /// Get list of configuration parameters.
        /// </summary>
        /// <response code="200">Configuration parameters</response>
        [HttpGet("parameters")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public SystemParametersDto GetParameters() => new();

        // Put.
        /// <summary>
        /// Force new validation of video manifest.
        /// </summary>
        /// <param name="hash">Hash manifest</param>
        [HttpPut("validate/manifest/{hash}")]
        [Authorize(CommonConsts.RequireAdministratorRolePolicy)]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task ForceVideoManifestValidationAsync(
            [Required] SwarmHash hash) =>
            service.ForceVideoManifestValidationAsync(hash);

        /// <summary>
        /// Force new validation of video manifests.
        /// </summary>
        /// <param name="id">Video id</param>
        [HttpPut("validate/video/{id}")]
        [Authorize(CommonConsts.RequireAdministratorRolePolicy)]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task ForceVideoManifestsValidationAsync(
            [Required] string id) =>
            service.ForceVideoManifestsValidationAsync(id);
    }
}
