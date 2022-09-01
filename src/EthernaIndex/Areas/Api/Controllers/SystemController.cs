using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Etherna.EthernaIndex.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class SystemController : ControllerBase
    {
        // Get.

        /// <summary>
        /// Get list of configuration parameters.
        /// </summary>
        /// <response code="200">Configuration parameters</response>
        [HttpGet("parameters")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Api requires a method")]
        public SystemParametersDto GetParameters() => new();
    }
}
