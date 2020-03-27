using Digicando.MongODM.Extensions;
using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Etherna.EthernaIndex.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<VideoDto> FindByHashAsync(string hash) =>
            new VideoDto(await indexContext.Videos.FindOneAsync(v => v.VideoHash == hash));

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take) =>
            (await indexContext.Videos.QueryElementsAsync(elements =>
                elements.PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(v => new VideoDto(v));

        public async Task<VideoDto> UpdateAsync(string videoHash, VideoUpdateInput videoInput)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.VideoHash == videoHash);

            video.SetDescription(videoInput.Description);
            video.SetThumbnailHash(videoInput.ThumbnailHash);
            video.SetTitle(videoInput.Title);

            await indexContext.SaveChangesAsync();

            return new VideoDto(video);
        }
    }
}
