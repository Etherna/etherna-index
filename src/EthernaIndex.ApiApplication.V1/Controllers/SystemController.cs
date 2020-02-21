using Etherna.EthernaIndex.ApiApplication.V1.Attributes;
using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            controllerService.MigrateDatabaseAsync();
    }
}
