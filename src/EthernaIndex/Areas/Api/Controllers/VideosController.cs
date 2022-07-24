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
    public class VideosController : ControllerBase
    {
        // Fields.
        private readonly IVideosControllerService service;

        // Constructors.
        public VideosController(IVideosControllerService controllerService)
        {
            this.service = controllerService;
        }

        // Get.

        /// <summary>
        /// Get video info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> FindByIdAsync(
            [Required] string id) =>
            service.FindByIdAsync(id, User);

        /// <summary>
        /// Get paginated video comments by id
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("{id}/comments")]
        [Obsolete("Use \"{id}/comments2\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(
            [Required] string id,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            (await service.GetVideoCommentsAsync(id, page, take)).Elements;

        /// <summary>
        /// Get paginated video comments by id
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("{id}/comments2")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<PaginatedEnumerableDto<CommentDto>> GetVideoComments2Async(
            [Required] string id,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideoCommentsAsync(id, page, take);

        /// <summary>
        /// Get validation info by id.
        /// </summary>
        /// <param name="id">The video id</param>
        [HttpGet("{id}/validations")]
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
        [Obsolete("Use \"latest2\" instead")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            (await service.GetLastUploadedVideosAsync(page, take)).Elements;

        /// <summary>
        /// Get list of last uploaded videos.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("latest2")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<PaginatedEnumerableDto<VideoDto>> GetLastUploadedVideos2Async(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetLastUploadedVideosAsync(page, take);

        /// <summary>
        /// Get video info by manifest hash.
        /// </summary>
        /// <param name="hash">The video hash</param>
        [HttpGet("manifest/{hash}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> FindByManifestHashAsync(
            [Required] string hash) =>
            service.FindByManifestHashAsync(hash, User);

        /// <summary>
        /// Get validation info by manifest hash.
        /// </summary>
        /// <param name="hash">The video hash</param>
        [HttpGet("manifest/{hash}/validation")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoManifestStatusDto> ValidationStatusByHashAsync(
            [Required] string hash) =>
            service.GetValidationStatusByHashAsync(hash);

        /// <summary>
        /// Search videos.
        /// </summary>
        /// <param name="title">Video title</param>
        /// <param name="description">Video description</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Videos</response>
        [HttpGet("Search")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<IEnumerable<VideoDto>> SearchVideoAsync(
            string? title,
            string? description,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.SearchVideoAsync(title, description, page, take);

        // Post.

        /// <summary>
        /// Create a new video with current user.
        /// </summary>
        /// <param name="videoInput">Info of new video</param>
        [HttpPost]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<string> CreateAsync(
            [Required] VideoCreateInput videoInput) =>
            service.CreateAsync(videoInput, User);

        /// <summary>
        /// Create a new comment on a video with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="text">Comment text</param>
        [HttpPost("{id}/comments")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<CommentDto> CreateCommentAsync(
            [Required] string id,
            [Required][FromBody] string text) =>
            service.CreateCommentAsync(id, text, User);

        /// <summary>
        /// Report a video content with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="hash">Hash manifest</param>
        /// <param name="description">Report description</param>
        [HttpPost("{id}/manifest/{hash}/reports")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task ReportVideoAsync(
            [Required] string id,
            [Required] string hash,
            [Required] string description) =>
            service.ReportVideoAsync(id, hash, description, User);

        /// <summary>
        /// Vote a video content with current user.
        /// </summary>
        /// <param name="id">Video id</param>
        /// <param name="value">Vote value</param>
        [HttpPost("{id}/votes")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task VoteVideAsync(
            [Required] string id,
            [Required] VoteValue value) =>
            service.VoteVideAsync(id, value, User);

        // Put.

        /// <summary>
        /// Update video manifest.
        /// </summary>
        /// <param name="id">The video id</param>
        /// <param name="newHash">The new video manifest hash</param>
        [HttpPut("{id}")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoManifestDto> UpdateAsync(
            [Required] string id,
            [Required] string newHash) =>
            service.UpdateAsync(id, newHash, User);

        // Delete.

        /// <summary>
        /// Delete a video from index. Only author is authorized
        /// </summary>
        /// <param name="id">Id of the video</param>
        [HttpDelete("{id}")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task AuthorDeleteAsync(
            [Required] string id) =>
            service.AuthorDeleteAsync(id, User);
    }
}
