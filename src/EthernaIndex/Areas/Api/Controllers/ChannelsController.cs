using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.Services;
using Etherna.EthernaIndex.Attributes;
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
        /// <param name="page">Current page of results</param>
        /// <param name="take">Number of items to retrieve. Max 100</param>
        /// <response code="200">List of channel's videos</response>
        /// <response code="404">Channel not found</response>
        [HttpGet("{address}/videos")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<VideoDto>> GetVideosAsync(
            [Required] string address,
            [Range(0, int.MaxValue)] int page,
            [Range(1, 100)] int take = 25) =>
            controllerService.GetVideosAsync(address, page, take);

        // Post.

        /// <summary>
        /// Create a new channel with current address.
        /// </summary>
        /// <response code="200">The new created channel</response>
        [HttpPost]
        [Authorize]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<ChannelDto> CreateAsync() =>
            controllerService.CreateAsync();
    }
}
