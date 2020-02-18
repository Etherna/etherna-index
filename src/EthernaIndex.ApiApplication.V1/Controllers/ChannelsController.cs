using Etherna.EthernaIndex.ApiApplication.V1.Attributes;
using Etherna.EthernaIndex.ApiApplication.V1.DtoModels;
using Etherna.EthernaIndex.ApiApplication.V1.InputModels;
using Etherna.EthernaIndex.ApiApplication.V1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ApiApplication.V1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChannelsController : ControllerBase
    {
        // Fields.
        private readonly IChannelsControllerService controllerService;

        // Constructors.
        public ChannelsController(IChannelsControllerService controllerService)
        {
            this.controllerService = controllerService;
        }

        // Get.

        /// <summary>
        /// Get a complete list of channels.
        /// </summary>
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">Current page on list</response>
        [HttpGet]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<IEnumerable<ChannelDto>> GetChannelsAsync(
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            controllerService.GetChannelsAsync(page, take);

        /// <summary>
        /// Get channel info by address.
        /// </summary>
        /// <param name="address">The channel address</param>
        [HttpGet("{address}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ChannelDto> FindByAddressAsync(
            string address) =>
            controllerService.FindByAddressAsync(address);

        /// <summary>
        /// Get list of videos uploaded by a channel.
        /// </summary>
        /// <param name="address">Address of the channel</param>
        /// <response code="200">List of channel's videos</response>
        /// <response code="404">Channel not found</response>
        [HttpGet("{address}/videos")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<VideoDto>> GetVideosAsync([Required] string address) =>
            controllerService.GetVideosAsync(address);

        // Post.

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <param name="channelInput">New channel data</param>
        /// <response code="200">The new created channel</response>
        [HttpPost]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<ChannelDto> CreateAsync([Required] ChannelInput channelInput) =>
            controllerService.CreateAsync(channelInput);

        /// <summary>
        /// Add a new video to a channel.
        /// </summary>
        /// <param name="address">Address of the channel</param>
        /// <param name="videoInput">Info of new video</param>
        /// <response code="404">Channel not found</response>
        [HttpPost("{address}/videos")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> AddVideoAsync(
            [Required] string address,
            [Required] VideoInput videoInput) =>
            controllerService.AddVideoAsync(address, videoInput);

        // Put.

        /// <summary>
        /// Update channel info.
        /// </summary>
        /// <param name="channelInput">Updated channel data</param>
        [HttpPut()]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ChannelDto> UpdateAsync([Required] ChannelInput channelInput) =>
            controllerService.UpdateAsync(channelInput);

        // Delete.

        /// <summary>
        /// Remove a video from a channel.
        /// </summary>
        /// <param name="address">Address of the channel</param>
        /// <param name="videoHash">Hash of the video</param>
        /// <response code="404">Channel not found</response>
        [HttpDelete("{address}/videos/{videoHash}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult> RemoveVideoAsync(
            [Required] string address,
            [Required] string videoHash) =>
            controllerService.RemoveVideoAsync(address, videoHash);
    }
}
