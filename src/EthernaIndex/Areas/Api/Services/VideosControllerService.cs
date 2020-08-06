using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongODM.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class VideosControllerService : IVideosControllerService
    {
        // Fields.
        private readonly IIndexContext indexContext;

        // Constructors.
        public VideosControllerService(IIndexContext indexContext)
        {
            this.indexContext = indexContext;
        }

        // Methods.
        public async Task<VideoDto> CreateAsync(string channelAddress, VideoCreateInput videoInput)
        {
            var channel = await indexContext.Channels.FindOneAsync(c => c.Address == channelAddress);
            var manifestHash = new SwarmContentHash(videoInput.ManifestHash);

            var video = new Video(
                videoInput.EncryptionKey,
                videoInput.EncryptionType,
                manifestHash,
                channel);

            await indexContext.Videos.CreateAsync(video);

            return new VideoDto(video);
        }

        public async Task<ActionResult> DeleteAsync(string hash)
        {
            var video = await indexContext.Videos.QueryElementsAsync(elements =>
                elements.FirstAsync(v => v.ManifestHash.Hash == hash));

            await indexContext.Videos.DeleteAsync(video);

            return new OkResult();
        }

        public async Task<VideoDto> FindByHashAsync(string hash) =>
            new VideoDto(await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == hash));

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take) =>
            (await indexContext.Videos.QueryElementsAsync(elements =>
                elements.PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(v => new VideoDto(v));
    }
}
