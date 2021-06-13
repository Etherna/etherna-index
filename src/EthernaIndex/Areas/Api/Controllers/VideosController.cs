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
    [ApiVersion("0.2")]
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
        /// Get list of last uploaded videos.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetLastUploadedVideosAsync(page, take);

        /// <summary>
        /// Get video info by hash.
        /// </summary>
        /// <param name="hash">The video hash</param>
        [HttpGet("{hash}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> FindByHashAsync(
            [Required] string hash) =>
            service.FindByHashAsync(hash);

        /// <summary>
        /// Get paginated video comments by hash
        /// </summary>
        /// <param name="hash">Video hash</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("{hash}/comments")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(
            [Required] string hash,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideoCommentsAsync(hash, page, take);

        /// <summary>
        /// Get video info by fairdrive path.
        /// </summary>
        /// <param name="path">The video path on FairDrive</param>
        [HttpGet("fdrive/{path}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> FindByFairDrivePathAsync(
            [Required] string path) =>
            service.FindByFairDrivePathAsync(Uri.UnescapeDataString(path));

        /// <summary>
        /// Get paginated video comments by fairdrive path
        /// </summary>
        /// <param name="path">The video path on FairDrive</param>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet("fdrive/{path}/comments")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<CommentDto>> GetVideoCommentsByFairDrivePathAsync(
            [Required] string path,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            service.GetVideoCommentsByFairDrivePathAsync(Uri.UnescapeDataString(path), page, take);

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
        public Task<VideoDto> CreateAsync(
            [Required] VideoCreateInput videoInput) =>
            service.CreateAsync(videoInput);

        /// <summary>
        /// Create a new comment on a video with current user.
        /// </summary>
        /// <param name="hash">Video hash</param>
        /// <param name="text">Comment text</param>
        [HttpPost("{hash}/comments")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<CommentDto> CreateCommentAsync(
            [Required] string hash,
            [Required][FromBody] string text) =>
            service.CreateCommentAsync(hash, text);

        /// <summary>
        /// Vote a video content with current user.
        /// </summary>
        /// <param name="hash">Video hash</param>
        /// <param name="value">Vote value</param>
        [HttpPost("{hash}/votes")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task VoteVideAsync(
            [Required] string hash,
            [Required] VoteValue value) =>
            service.VoteVideAsync(hash, value);

        /// <summary>
        /// Create a new comment on a video with current user.
        /// </summary>
        /// <param name="path">The video path on FairDrive</param>
        /// <param name="text">Comment text</param>
        [HttpPost("fdrive/{path}/comments")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task<CommentDto> CreateCommentByFairDrivePathAsync(
            [Required] string path,
            [Required][FromBody] string text) =>
            service.CreateCommentByFairDrivePathAsync(Uri.UnescapeDataString(path), text);

        /// <summary>
        /// Vote a video content with current user.
        /// </summary>
        /// <param name="path">The video path on FairDrive</param>
        /// <param name="value">Vote value</param>
        [HttpPost("fdrive/{path}/votes")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public Task VoteVideByFairDrivePathAsync(
            [Required] string path,
            [Required] VoteValue value) =>
            service.VoteVideByFairDrivePathAsync(Uri.UnescapeDataString(path), value);

        // Put.

        /// <summary>
        /// Update video manifest.
        /// </summary>
        /// <param name="oldHash">The old video manifest hash</param>
        /// <param name="newHash">The new video manifest hash</param>
        [HttpPut("{oldHash}")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> UpdateAsync(
            [Required] string oldHash,
            [Required] string newHash) =>
            service.UpdateAsync(oldHash, newHash);

        // Delete.

        /// <summary>
        /// Delete a video from index.
        /// </summary>
        /// <param name="hash">Hash of the video</param>
        [HttpDelete("{hash}")]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task DeleteAsync(
            [Required] string hash) =>
            service.DeleteAsync(hash);
    }
}
