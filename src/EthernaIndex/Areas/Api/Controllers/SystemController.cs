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
    public class SystemController : ControllerBase
    {
        // Fields.
        private readonly ISystemControllerService service;

        // Constructor.
        public SystemController(
            ISystemControllerService service)
        {
            this.service = service;
        }

        // Get.
        /// <summary>
        /// Get list of configuration parameters.
        /// </summary>
        /// <response code="200">Configuration parameters</response>
        [HttpGet("parameters")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public SystemParametersDto GetParameters() => new();

        // Put.
        /// <summary>
        /// Force new validation of video manifest.
        /// </summary>
        /// <param name="hash">Hash manifest</param>
        [HttpPut("validate/manifest/{hash}")]
        [Authorize(CommonConsts.RequireAdministratorClaimPolicy)]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task ForceVideoManifestValidationAsync(
            [Required] string hash) =>
            service.ForceVideoManifestValidationAsync(hash);

        /// <summary>
        /// Force new validation of video manifests.
        /// </summary>
        /// <param name="id">Video id</param>
        [HttpPut("validate/video/{id}")]
        [Authorize(CommonConsts.RequireAdministratorClaimPolicy)]
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
