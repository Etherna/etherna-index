using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IVideosControllerService
    {
        Task<VideoDto> CreateAsync(VideoCreateInput videoInput);
        Task<CommentDto> CreateCommentAsync(string hash, string text);
        Task<CommentDto> CreateCommentByFairDrivePathAsync(string path, string text);
        Task DeleteAsync(string hash);
        Task<VideoDto> FindByHashAsync(string hash);
        Task<VideoDto> FindByFairDrivePathAsync(string path);
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string hash, int page, int take);
        Task<IEnumerable<CommentDto>> GetVideoCommentsByFairDrivePathAsync(string path, int page, int take);
        Task<VideoDto> UpdateAsync(string oldHash, string newHash);
        Task VoteVideAsync(string hash, VoteValue value);
        Task VoteVideByFairDrivePathAsync(string path, VoteValue value);
    }
}