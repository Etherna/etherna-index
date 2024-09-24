// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.BeeNet.Models;
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
        Task<Video2Dto> FindByManifestHashAsync(SwarmHash hash);
        Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByHashesAsync(IEnumerable<SwarmHash> manifestHashes);
        Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByIdsAsync(IEnumerable<string> ids);
        Task<PaginatedEnumerableDto<VideoPreviewDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<VideoManifestStatusDto> GetValidationStatusByHashAsync(SwarmHash manifestHash);
        Task<IEnumerable<VideoManifestStatusDto>> GetValidationStatusByIdAsync(string id);
        Task<PaginatedEnumerableDto<Comment2Dto>> GetVideoCommentsAsync(string id, int page, int take);
        Task ReportVideoAsync(string videoId, SwarmHash manifestHash, string description);
        Task<VideoManifest2Dto> UpdateAsync(string id, SwarmHash newHash);
        Task UpdateCommentAsync(string commentId, string text);
        Task VoteVideAsync(string id, VoteValue value);

        //deprecated
        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoDto> FindByIdAsync_old(string id);

        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoDto> FindByManifestHashAsync_old(SwarmHash hash);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IEnumerable<VideoStatusDto>> GetBulkValidationStatusByIdsAsync_old(IEnumerable<string> ids);

        [Obsolete("Used only for API backwards compatibility")]
        Task<PaginatedEnumerableDto<VideoDto>> GetLastUploadedVideosAsync_old(int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoStatusDto> GetValidationStatusByIdAsync_old(string id);
        
        [Obsolete("Used only for API backwards compatibility")]
        Task<PaginatedEnumerableDto<CommentDto>> GetVideoCommentsAsync_old(string id, int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<VideoManifestDto> UpdateAsync_old(string id, SwarmHash newHash);
    }
}