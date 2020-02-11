using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    public class VideosControllerService : IVideosControllerService
    {
        public Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take)
        {
            throw new NotImplementedException();
        }
    }
}
