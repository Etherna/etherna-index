using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IVideosControllerService
    {
        Task<VideoDto> CreateAsync(VideoCreateInput videoInput);
        Task<CommentDto> CreateCommentAsync(string hash, string text);
        Task DeleteAsync(string hash);
        Task<VideoDto> FindByHashAsync(string hash);
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string hash, int page, int take);
        Task<VideoDto> UpdateAsync(string oldHash, string newHash);
    }
}