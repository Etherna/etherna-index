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
    public class ModerationController(IModerationControllerService service) : ControllerBase
    {
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
        [HttpDelete("videos/{id}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task ModerateVideoAsync(
            [Required] string id) =>
            service.ModerateVideoAsync(id);
    }
}
