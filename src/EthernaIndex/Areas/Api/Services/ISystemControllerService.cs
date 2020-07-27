using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface ISystemControllerService
    {
        Task<SettingsDto> GetSettingsAsync();
        Task<IActionResult> MigrateDatabaseAsync(string currentUserAddress);
    }
}