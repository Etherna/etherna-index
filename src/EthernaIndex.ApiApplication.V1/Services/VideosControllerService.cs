using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Etherna.EthernaIndex.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Services
{
    internal class VideosControllerService : IVideosControllerService
    {
        // Fields.
        private readonly IIndexContext indexContext;

        // Constructors.
        public VideosControllerService(IIndexContext indexContext)
        {
            this.indexContext = indexContext;
        }

        // Methods.
        public Task<VideoDto> FindByHashAsync(string hash)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take)
        {
            throw new NotImplementedException();
        }

        public Task<VideoDto> UpdateAsync(string hash, VideoInput videoInput)
        {
            throw new NotImplementedException();
        }
    }
}
