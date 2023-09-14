//   Copyright 2021-present Etherna Sagl
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IVideosControllerService
    {
        Task AuthorDeleteAsync(string id);
        Task<string> CreateAsync(VideoCreateInput videoInput);
        Task<Comment2Dto> CreateCommentAsync(string id, string text);
        Task<Video2Dto> FindByIdAsync(string id);
        Task<Video2Dto> FindByManifestHashAsync(string hash);
        Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByHashesAsync(IEnumerable<string> manifestHashes);
        Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByIdsAsync(IEnumerable<string> ids);
        Task<PaginatedEnumerableDto<VideoPreviewDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<VideoManifestStatusDto> GetValidationStatusByHashAsync(string manifestHash);
        Task<IEnumerable<VideoManifestStatusDto>> GetValidationStatusByIdAsync(string id);
        Task<PaginatedEnumerableDto<Comment2Dto>> GetVideoCommentsAsync(string id, int page, int take);
        Task ReportVideoAsync(string videoId, string manifestHash, string description);
        Task<VideoManifest2Dto> UpdateAsync(string id, string newHash);
        Task UpdateCommentAsync(string commentId, string text);
        Task VoteVideAsync(string id, VoteValue value);

        //deprecated
        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoDto> FindByIdAsync_old(string id);

        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoDto> FindByManifestHashAsync_old(string hash);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IEnumerable<VideoStatusDto>> GetBulkValidationStatusByIdsAsync_old(IEnumerable<string> ids);

        [Obsolete("Used only for API backwards compatibility")]
        Task<PaginatedEnumerableDto<VideoDto>> GetLastUploadedVideosAsync_old(int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoStatusDto> GetValidationStatusByIdAsync_old(string id);
        
        [Obsolete("Used only for API backwards compatibility")]
        Task<PaginatedEnumerableDto<CommentDto>> GetVideoCommentsAsync_old(string id, int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoManifestDto> UpdateAsync_old(string id, string newHash);
    }
}