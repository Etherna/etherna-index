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

using Asp.Versioning;
using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Areas.Api.Services;
using Etherna.EthernaIndex.Attributes;
using Etherna.EthernaIndex.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Controllers
{
    [ApiController]
    [ApiVersion("0.3")]
    [Route("api/v{api-version:apiVersion}/[controller]")]
    public class VideosController(IVideosControllerService service) : ControllerBase
    {
        // Get.

        /// <summary>
        /// Get video info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}")]
        [Obsolete("Use \"find2\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> FindByIdAsync_old(
            [Required] string id) =>
            service.FindByIdAsync_old(id);

        /// <summary>
        /// Get video info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}/find2")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<Video2Dto> FindByIdAsync(
            [Required] string id) =>
            service.FindByIdAsync(id);

        /// <summary>
        /// Get paginated video comments by id
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("{id}/comments")]
        [Obsolete("Use \"{id}/comments3\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(
            [Required] string id,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            (await service.GetVideoCommentsAsync_old(id, page, take)).Elements;

        /// <summary>
        /// Get paginated video comments by id
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("{id}/comments2")]
        [Obsolete("Use \"{id}/comments3\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<PaginatedEnumerableDto<CommentDto>> GetVideoComments2Async(
            [Required] string id,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideoCommentsAsync_old(id, page, take);
        
        /// <summary>
        /// Get paginated video comments by id
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("{id}/comments3")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<PaginatedEnumerableDto<Comment2Dto>> GetVideoComments3Async(
            [Required] string id,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideoCommentsAsync(id, page, take);

        /// <summary>
        /// Get validation info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}/validations")]
        [Obsolete("Use \"{id}/validation2\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<VideoManifestStatusDto>> ValidationsStatusByIdAsync_old(
            [Required] string id) =>
            (await service.GetValidationStatusByIdAsync_old(id)).ManifestsStatus;

        /// <summary>
        /// Get validation info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}/validation")]
        [Obsolete("Use \"{id}/validation2\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoStatusDto> ValidationsStatusByIdAsync_old2(
            [Required] string id) =>
            service.GetValidationStatusByIdAsync_old(id);

        /// <summary>
        /// Get validation info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}/validation2")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<VideoManifestStatusDto>> ValidationsStatusByIdAsync(
            [Required] string id) =>
            service.GetValidationStatusByIdAsync(id);

        /// <summary>
        /// Get list of last uploaded videos.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("latest")]
        [Obsolete("Use \"latest3\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync_old0(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            (await service.GetLastUploadedVideosAsync_old(page, take)).Elements;

        /// <summary>
        /// Get list of last uploaded videos.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("latest2")]
        [Obsolete("Use \"latest3\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<PaginatedEnumerableDto<VideoDto>> GetLastUploadedVideosAsync_old1(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetLastUploadedVideosAsync_old(page, take);

        /// <summary>
        /// Get list of last uploaded videos.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("latest3")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<PaginatedEnumerableDto<VideoPreviewDto>> GetLastUploadedVideosAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetLastUploadedVideosAsync(page, take);

        /// <summary>
        /// Get video info by manifest hash.
        /// </summary>
        /// <param name="hash">The video hash</param>
        [HttpGet("manifest/{hash}")]
        [Obsolete("Use \"manifest2\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> FindByManifestHashAsync_old(
            [Required] SwarmHash hash) =>
            service.FindByManifestHashAsync_old(hash);

        /// <summary>
        /// Get video info by manifest hash.
        /// </summary>
        /// <param name="hash">The video hash</param>
        [HttpGet("manifest2/{hash}")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<Video2Dto> FindByManifestHashAsync(
            [Required] SwarmHash hash) =>
            service.FindByManifestHashAsync(hash);

        /// <summary>
        /// Get validation info by manifest hash.
        /// </summary>
        /// <param name="hash">The video hash</param>
        [HttpGet("manifest/{hash}/validation")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoManifestStatusDto> ValidationStatusByHashAsync(
            [Required] SwarmHash hash) =>
            service.GetValidationStatusByHashAsync(hash);

        // Post.

        /// <summary>
        /// Create a new video from manifest hash
        /// </summary>
        /// <param name="input">Info of new video</param>
        [HttpPost]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")] //force because of https://github.com/RicoSuter/NSwag/issues/4132
        public Task<string> CreateAsync(
            [Required] VideoCreateInput input)
        {
            ArgumentNullException.ThrowIfNull(input, nameof(input));
            return service.CreateFromManifestAsync(input.ManifestHash);
        }

        /// <summary>
        /// Create a new video from hls playlist hash
        /// </summary>
        /// <param name="input">Info of new video</param>
        [HttpPost]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Produces("application/json")] //force because of https://github.com/RicoSuter/NSwag/issues/4132
        public Task<string> CreateFromRawHlsAsync(
            VideoCreateFromRawInput input) =>
            service.CreateFromRawHlsAsync(input);

        /// <summary>
        /// Create a new comment on a video with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="text">Comment text</param>
        [HttpPost("{id}/comments")]
        [Obsolete("Use \"{id}/comments2\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<CommentDto> CreateCommentAsync(
            [Required] string id,
            [Required][FromBody] string text) =>
            new(await service.CreateCommentAsync(id, text));

        /// <summary>
        /// Create a new comment on a video with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="text">Comment text</param>
        [HttpPost("{id}/comments2")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<Comment2Dto> CreateComment2Async(
            [Required] string id,
            [Required][FromBody] string text) =>
            service.CreateCommentAsync(id, text);

        /// <summary>
        /// Report a video content with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="hash">Hash manifest</param>
        /// <param name="description">Report description</param>
        [HttpPost("{id}/manifest/{hash}/reports")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task ReportVideoAsync(
            [Required] string id,
            [Required] SwarmHash hash,
            [Required] string description) =>
            service.ReportVideoAsync(id, hash, description);

        /// <summary>
        /// Vote a video content with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="value">Vote value</param>
        [HttpPost("{id}/votes")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task VoteVideAsync(
            [Required] string id,
            [Required] VoteValue value) =>
            service.VoteVideAsync(id, value);

        // Put.

        /// <summary>
        /// Get bulk validation info by multiple manifest hashes.
        /// </summary>
        /// <param name="hashes">The list of video manifest hashes</param>
        [HttpPut("manifest/bulkValidation")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByHashesAsync(
            [Required][FromBody] IEnumerable<SwarmHash> hashes) =>
            service.GetBulkValidationStatusByHashesAsync(hashes);

        /// <summary>
        /// Get bulk validation info by multiple video ids.
        /// </summary>
        /// <param name="ids">The list of video id</param>
        [HttpPut("bulkValidation")]
        [Obsolete("Use \"bulkValidation2\" instead")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IEnumerable<VideoStatusDto>> GetBulkValidationStatusByIdsAsync_old(
            [Required][FromBody] IEnumerable<string> ids) =>
            service.GetBulkValidationStatusByIdsAsync_old(ids);

        /// <summary>
        /// Get bulk validation info by multiple video ids.
        /// </summary>
        /// <param name="ids">The list of video id</param>
        [HttpPut("bulkValidation2")]
        [AllowAnonymous]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByIdsAsync(
            [Required][FromBody] IEnumerable<string> ids) =>
            service.GetBulkValidationStatusByIdsAsync(ids);

        /// <summary>
        /// Edit a video comment with current author user.
        /// </summary>
        /// <param name="commentId">Comment id</param>
        /// <param name="text">Comment text</param>
        [HttpPut("comments/{commentId}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task UpdateCommentAsync(
            [Required] string commentId,
            [Required][FromBody] string text) =>
            service.UpdateCommentAsync(commentId, text);

        /// <summary>
        /// Update video manifest.
        /// </summary>
        /// <param name="id">The video id</param>
        /// <param name="newHash">The new video manifest hash</param>
        [HttpPut("{id}")]
        [Obsolete("Use \"update2\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoManifestDto> UpdateAsync_old(
            [Required] string id,
            [Required] SwarmHash newHash) =>
            service.UpdateAsync_old(id, newHash);

        /// <summary>
        /// Update video manifest.
        /// </summary>
        /// <param name="id">The video id</param>
        /// <param name="newHash">The new video manifest hash</param>
        [HttpPut("{id}/update2")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoManifest2Dto> UpdateAsync(
            [Required] string id,
            [Required] SwarmHash newHash) =>
            service.UpdateAsync(id, newHash);

        // Delete.

        /// <summary>
        /// Delete a video from index. Only author is authorized
        /// </summary>
        /// <param name="id">Id of the video</param>
        [HttpDelete("{id}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task AuthorDeleteAsync(
            [Required] string id) =>
            service.AuthorDeleteAsync(id);
    }
}
