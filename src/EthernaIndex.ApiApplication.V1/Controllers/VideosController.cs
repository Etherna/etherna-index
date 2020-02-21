﻿using Etherna.EthernaIndex.ApiApplication.V1.Attributes;
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
    public class VideosController : ControllerBase
    {
        // Fields.
        private readonly IVideosControllerService controllerService;

        // Constructors.
        public VideosController(IVideosControllerService controllerService)
        {
            this.controllerService = controllerService;
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
            controllerService.GetLastUploadedVideosAsync(page, take);

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
            string hash) =>
            controllerService.FindByHashAsync(hash);

        // Put.

        /// <summary>
        /// Update video info.
        /// </summary>
        /// <param name="hash">The video hash</param>
        /// <param name="videoInput">Updated video info</param>
        [HttpPut("{hash}")]
        [SimpleExceptionFilter]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<VideoDto> UpdateAsync(string hash, [Required] VideoUpdateInput videoInput) =>
            controllerService.UpdateAsync(hash, videoInput);
    }
}