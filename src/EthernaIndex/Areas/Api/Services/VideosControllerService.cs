﻿using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.EthernaIndex.Extensions;
using Etherna.MongODM.Extensions;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class VideosControllerService : IVideosControllerService
    {
        // Fields.
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIndexContext indexContext;

        // Constructors.
        public VideosControllerService(
            IHttpContextAccessor httpContextAccessor,
            IIndexContext indexContext)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.indexContext = indexContext;
        }

        // Methods.
        public async Task<VideoDto> CreateAsync(VideoCreateInput videoInput)
        {
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(c => c.Address == address);
            var manifestHash = new SwarmContentHash(videoInput.ManifestHash);

            var video = new Video(
                videoInput.EncryptionKey,
                videoInput.EncryptionType,
                manifestHash,
                user);

            await indexContext.Videos.CreateAsync(video);

            return new VideoDto(video);
        }

        public async Task DeleteAsync(string hash)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var video = await indexContext.Videos.QueryElementsAsync(elements =>
                elements.FirstAsync(v => v.ManifestHash.Hash == hash));

            // Verify authz.
            if (!video.Owner.Address.IsTheSameAddress(address))
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Action.
            await indexContext.Videos.DeleteAsync(video);
        }

        public async Task<VideoDto> FindByHashAsync(string hash) =>
            new VideoDto(await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == hash));

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take) =>
            (await indexContext.Videos.QueryElementsAsync(elements =>
                elements.PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(v => new VideoDto(v));

        public async Task<VideoDto> UpdateAsync(string oldHash, string newHash)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var video = await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == oldHash);

            // Verify authz.
            if (!video.Owner.Address.IsTheSameAddress(address))
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Action.
            video.SetManifestHash(new SwarmContentHash(newHash));
            await indexContext.SaveChangesAsync();

            return new VideoDto(video);
        }
    }
}
