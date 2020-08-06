using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IVideosControllerService
    {
        Task<VideoDto> CreateAsync(string channelAddress, VideoCreateInput videoInput);
        Task<ActionResult> DeleteAsync(string hash);
        Task<VideoDto> FindByHashAsync(string hash);
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<VideoDto> UpdateAsync(string oldHash, string newHash);
    }
}