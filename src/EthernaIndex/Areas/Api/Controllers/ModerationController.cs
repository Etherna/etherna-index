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
    [Authorize(CommonConsts.RequireAdministratorClaimPolicy)]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class ModerationController : ControllerBase
    {
        // Fields.
        private readonly IModerationControllerService service;

        // Constructor.
        public ModerationController(IModerationControllerService service)
        {
            this.service = service;
        }

        // Delete.

        /// <summary>
        /// Moderate comment as unsittable for the index
        /// </summary>
        /// <param name="id">Id of the comment</param>
        [HttpDelete("comments/{id}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task ModerateCommentAsync(
            [Required] string id) =>
            service.ModerateCommentAsync(id);

        /// <summary>
        /// Moderate video as unsittable for the index
        /// </summary>
        /// <param name="id">Id of the video</param>
        /// <param name="description">Moderation reason</param>
        [HttpDelete("videos/{id}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task ModerateVideoAsync(
            [Required] string id,
            [Required] [FromBody] string description) =>
            service.ModerateVideoAsync(id, description);
    }
}
