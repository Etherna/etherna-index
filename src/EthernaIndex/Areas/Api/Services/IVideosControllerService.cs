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
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface IVideosControllerService
    {
        Task<string> CreateAsync(VideoCreateInput videoInput, ClaimsPrincipal userClaims);
        Task<CommentDto> CreateCommentAsync(string id, string text, ClaimsPrincipal userClaims);
        Task DeleteAsync(string id, ClaimsPrincipal userClaims);
        Task<VideoDto> FindByIdAsync(string id, ClaimsPrincipal userClaims);
        Task<VideoDto> FindByManifestHashAsync(string hash, ClaimsPrincipal userClaims);
        Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take);
        Task<ManifestStatusDto> GetValidationStatusByHashAsync(string manifestHash);
        Task<IEnumerable<ManifestStatusDto>> GetValidationStatusByIdAsync(string id);
        Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string id, int page, int take);
        Task ReportVideoAsync(string videoId, string manifestHash, string description, ClaimsPrincipal userClaims);
        Task<VideoManifestDto> UpdateAsync(string id, string newHash, ClaimsPrincipal userClaims);
        Task VoteVideAsync(string id, VoteValue value, ClaimsPrincipal userClaims);
    }
}