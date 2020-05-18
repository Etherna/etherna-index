using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class SystemControllerService : ISystemControllerService
    {
        // Fields.
        private readonly IConfiguration configuration;
        private readonly IIndexContext indexContext;

        // Constructors.
        public SystemControllerService(
            IConfiguration configuration,
            IIndexContext indexContext)
        {
            this.configuration = configuration;
            this.indexContext = indexContext;
        }

        // Methods.
        public Task<SettingsDto> GetSettingsAsync() =>
            Task.FromResult(new SettingsDto(
                configuration["SWARM_DEAFULT_GATEWAY"],
                "0.1"));

        public async Task<IActionResult> MigrateDatabaseAsync()
        {
            await indexContext.MigrateRepositoriesAsync();
            return new OkResult();
        }
    }
}
