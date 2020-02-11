using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    public interface IVideosControllerService
    {
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
    }
}