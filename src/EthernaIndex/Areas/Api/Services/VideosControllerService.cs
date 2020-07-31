using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
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
            var video = new Video(
                videoInput.Description,
                videoInput.EncryptionKey,
                videoInput.EncryptionType,
                TimeSpan.FromSeconds(videoInput.LengthInSeconds),
                channel,
                videoInput.ThumbnailHash,
                videoInput.ThumbnailHashIsRaw,
                videoInput.Title,
                videoInput.VideoHash,
                videoInput.VideoHashIsRaw);

            await indexContext.Videos.CreateAsync(video);

            return new VideoDto(video);
        }

        public async Task<ActionResult> DeleteAsync(string hash)
        {
            var video = await indexContext.Videos.QueryElementsAsync(elements =>
                elements.FirstAsync(v => v.VideoHash == hash));

            await indexContext.Videos.DeleteAsync(video);

            return new OkResult();
        }

        public async Task<VideoDto> FindByHashAsync(string hash) =>
            new VideoDto(await indexContext.Videos.FindOneAsync(v => v.VideoHash == hash));

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take) =>
            (await indexContext.Videos.QueryElementsAsync(elements =>
                elements.PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(v => new VideoDto(v));

        public async Task<VideoDto> UpdateAsync(string videoHash, VideoUpdateInput videoInput)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.VideoHash == videoHash);

            video.SetDescription(videoInput.Description);
            video.ThumbnailHash = videoInput.ThumbnailHash;
            video.SetTitle(videoInput.Title);

            await indexContext.SaveChangesAsync();

            return new VideoDto(video);
        }
    }
}
