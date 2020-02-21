using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    internal class SystemControllerService : ISystemControllerService
    {
        // Fields.
        private readonly IIndexContext indexContext;

        // Constructors.
        public SystemControllerService(IIndexContext indexContext)
        {
            this.indexContext = indexContext;
        }

        // Methods.
        public Task<SettingsDto> GetSettingsAsync() =>
            Task.FromResult(new SettingsDto("swarmGatewayUrl"));

        public async Task<IActionResult> MigrateDatabaseAsync()
        {
            await indexContext.MigrateRepositoriesAsync();
            return new OkResult();
        }
    }
}
