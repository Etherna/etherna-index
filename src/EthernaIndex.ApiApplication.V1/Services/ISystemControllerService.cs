using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    public interface ISystemControllerService
    {
        Task<SettingsDto> GetSettingsAsync();
        Task<IActionResult> MigrateDatabaseAsync();
    }
}