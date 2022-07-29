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
        /// Set a video as unsittable for the index
        /// </summary>
        /// <param name="id">Id of the video</param>
        [HttpDelete("videos/{id}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task RemoveVideoAsync(
            [Required] string id) =>
            service.RemoveVideoAsync(id);
    }
}
