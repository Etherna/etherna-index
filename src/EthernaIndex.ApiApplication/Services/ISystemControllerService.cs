using Etherna.EthernaIndex.ApiApplication.DtoModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.Services
{
    public interface ISystemControllerService
    {
        Task<SettingsDto> GetSettingsAsync();
        Task<IActionResult> MigrateDatabaseAsync();
    }
}