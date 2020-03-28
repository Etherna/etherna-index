using Etherna.EthernaIndex.ApiApplication.DtoModels;
using Etherna.EthernaIndex.ApiApplication.InputModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.Services
{
    public interface IVideosControllerService
    {
        Task<VideoDto> FindByHashAsync(string hash);
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<VideoDto> UpdateAsync(string videoHash, VideoUpdateInput videoInput);
    }
}