using Etherna.EthernaIndex.Areas.Api.Services;
using Etherna.EthernaIndex.Attributes;
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
    public class CommentsController : ControllerBase
    {
        // Fields.
        private readonly ICommentsControllerService service;

        // Constructor.
        public CommentsController(
            ICommentsControllerService service)
        {
            this.service = service;
        }

        // Delete.

        /// <summary>
        /// Delete a comment owned by current user.
        /// </summary>
        /// <param name="id">Comment id</param>
        [HttpDelete("{id}")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task DeleteOwnedCommentAsync(
            [Required] string id) =>
            service.DeleteOwnedCommentAsync(id, User);
    }
}
