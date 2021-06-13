using Etherna.EthernaIndex.Areas.Api.DtoModels;
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
                videoInput.FairDrivePath,
                manifestHash,
                user);

            await indexContext.Videos.CreateAsync(video);

            return new VideoDto(video);
        }

        public async Task<CommentDto> CreateCommentAsync(string hash, string text)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == hash);
            return await CreateCommentHelperAsync(text, video);
        }

        public async Task<CommentDto> CreateCommentByFairDrivePathAsync(string path, string text)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.FairDrivePath == path);
            return await CreateCommentHelperAsync(text, video);
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

        public async Task<VideoDto> FindByFairDrivePathAsync(string path) =>
            new VideoDto(await indexContext.Videos.FindOneAsync(v => v.FairDrivePath == path));

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take) =>
            (await indexContext.Videos.QueryElementsAsync(elements =>
                elements.PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(v => new VideoDto(v));

        public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string hash, int page, int take) =>
            (await indexContext.Comments.QueryElementsAsync(elements =>
                elements.Where(c => c.Video.ManifestHash.Hash == hash)
                        .PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(c => new CommentDto(c));

        public async Task<IEnumerable<CommentDto>> GetVideoCommentsByFairDrivePathAsync(string path, int page, int take) =>
            (await indexContext.Comments.QueryElementsAsync(elements =>
                elements.Where(c => c.Video.FairDrivePath == path)
                        .PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(c => new CommentDto(c));

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

        public async Task VoteVideAsync(string hash, VoteValue value)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == hash);
            await VoteVideoHelperAsync(value, video);
        }

        public async Task VoteVideByFairDrivePathAsync(string path, VoteValue value)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.FairDrivePath == path);
            await VoteVideoHelperAsync(value, video);
        }

        // Helpers.
        private async Task<CommentDto> CreateCommentHelperAsync(string text, Video video)
        {
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(u => u.Address == address);

            var comment = new Comment(user, text, video);

            await indexContext.Comments.CreateAsync(comment);

            return new CommentDto(comment);
        }

        private async Task VoteVideoHelperAsync(VoteValue value, Video video)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(u => u.Address == address);

            // Remove prev votes of user on this content.
            var prevVotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Owner.Address == address && v.Video.ManifestHash.Hash == video.ManifestHash.Hash)
                        .ToListAsync());
            foreach (var prevVote in prevVotes)
                await indexContext.Votes.DeleteAsync(prevVote);

            // Create new vote.
            var vote = new VideoVote(user, video, value);
            await indexContext.Votes.CreateAsync(vote);

            // Update counters on video.
            var totDownvotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.ManifestHash.Hash == video.ManifestHash.Hash && v.Value == VoteValue.Down)
                        .LongCountAsync());
            var totUpvotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.ManifestHash.Hash == video.ManifestHash.Hash && v.Value == VoteValue.Up)
                        .LongCountAsync());

            video.TotDownvotes = totDownvotes;
            video.TotUpvotes = totUpvotes;

            await indexContext.SaveChangesAsync();
        }
    }
}
