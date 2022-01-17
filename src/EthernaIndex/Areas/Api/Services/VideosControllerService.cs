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

using Etherna.Authentication.Extensions;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.EthernaIndex.Services.Exceptions;
using Etherna.EthernaIndex.Services.Interfaces;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Extensions;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Http;
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
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IIndexContext indexContext;

        // Constructors.
        public VideosControllerService(
            IHttpContextAccessor httpContextAccessor,
            IIndexContext indexContext,
            IBackgroundJobClient backgroundJobClient)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.indexContext = indexContext;
            this.backgroundJobClient = backgroundJobClient;
        }

        // Methods.
        public async Task<VideoDto> CreateAsync(VideoCreateInput videoInput)
        {
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(c => c.Address == address);
            var validationResult = await indexContext.VideoValidationResults.TryFindOneAsync(c => c.ManifestHash.Hash == videoInput.ManifestHash);

            if (validationResult is not null)
            {
                throw new DuplicatedManifestHashException(videoInput.ManifestHash);
            }

            await indexContext.VideoValidationResults.CreateAsync(new VideoValidationResult(videoInput.ManifestHash, user));

            backgroundJobClient.Create<MetadataVideoValidatorTask>(
                task => task.RunAsync(videoInput.ManifestHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            var manifestHash = new SwarmContentHash(videoInput.ManifestHash);

            var video = new Video(
                videoInput.EncryptionKey,
                videoInput.EncryptionType,
                manifestHash,
                user);

            await indexContext.Videos.CreateAsync(video);

            return new VideoDto(video);
        }

        public async Task<CommentDto> CreateCommentAsync(string hash, string text)
        {
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(u => u.Address == address);
            var video = await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == hash);

            var comment = new Comment(user, text, video);

            await indexContext.Comments.CreateAsync(comment);

            return new CommentDto(comment);
        }

        public async Task DeleteAsync(string hash)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
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

        public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string hash, int page, int take) =>
            (await indexContext.Comments.QueryElementsAsync(elements =>
                elements.Where(c => c.Video.ManifestHash.Hash == hash)
                        .PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(c => new CommentDto(c));

        public async Task<VideoDto> UpdateAsync(string oldHash, string newHash)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
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
            // Get data.
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(u => u.Address == address);
            var video = await indexContext.Videos.FindOneAsync(v => v.ManifestHash.Hash == hash);

            // Remove prev votes of user on this content.
            var prevVotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Owner.Address == address && v.Video.ManifestHash.Hash == hash)
                        .ToListAsync());
            foreach (var prevVote in prevVotes)
                await indexContext.Votes.DeleteAsync(prevVote);

            // Create new vote.
            var vote = new VideoVote(user, video, value);
            await indexContext.Votes.CreateAsync(vote);

            // Update counters on video.
            var totDownvotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.ManifestHash.Hash == hash && v.Value == VoteValue.Down)
                        .LongCountAsync());
            var totUpvotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.ManifestHash.Hash == hash && v.Value == VoteValue.Up)
                        .LongCountAsync());

            video.TotDownvotes = totDownvotes;
            video.TotUpvotes = totUpvotes;

            await indexContext.SaveChangesAsync();
        }
    }
}
