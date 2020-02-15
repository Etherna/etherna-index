using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    public interface IVideosControllerService
    {
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<VideoDto> UpdateAsync(string hash, VideoInput videoInput);
        Task<VideoDto> FindByHashAsync(string hash);
    }
}