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
    [Authorize]
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
        [SimpleExceptionFilter]
        public Task<IActionResult> MigrateDatabase() =>
            controllerService.MigrateDatabaseAsync(User.GetEtherAddress());
    }
}
