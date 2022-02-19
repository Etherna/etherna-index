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
using Etherna.EthernaIndex.Areas.Api.DtoModels.ManifestAgg;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Services.Exceptions;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Extensions;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        private readonly IIndexDbContext indexContext;
        private readonly ILogger<VideosControllerService> logger;

        // Constructors.
        public VideosControllerService(
            IBackgroundJobClient backgroundJobClient,
            IHttpContextAccessor httpContextAccessor,
            IIndexDbContext indexContext,
            ILogger<VideosControllerService> logger)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.httpContextAccessor = httpContextAccessor;
            this.indexContext = indexContext;
            this.logger = logger;
        }

        // Methods.
        public async Task<string> CreateAsync(VideoCreateInput videoInput)
        {
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(c => c.Address == address);
            var videoManifest = await indexContext.VideoManifests.TryFindOneAsync(c => c.ManifestHash.Hash == videoInput.ManifestHash);

            if (videoManifest is not null)
            {
                throw new DuplicatedManifestHashException(videoInput.ManifestHash);
            }

            // Create Video.
            var video = new Video(
                videoInput.EncryptionKey,
                videoInput.EncryptionType,
                user);

            await indexContext.Videos.CreateAsync(video);

            // Create video manifest.
            videoManifest = new VideoManifest(videoInput.ManifestHash, video);
            await indexContext.VideoManifests.CreateAsync(videoManifest);

            // Create Validation Manifest Task.
            backgroundJobClient.Create<MetadataVideoValidatorTask>(
                task => task.RunAsync(video.Id, videoInput.ManifestHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.CreatedVideo(user.Id, videoInput.ManifestHash);

            return video.Id;
        }

        public async Task<CommentDto> CreateCommentAsync(string id, string text)
        {
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(u => u.Address == address);
            var video = await indexContext.Videos.FindOneAsync(id);

            var comment = new Comment(user, text, video);

            await indexContext.Comments.CreateAsync(comment);

            logger.CreatedCommentVideo(user.Id, id);

            return new CommentDto(comment);
        }

        public async Task DeleteAsync(string id)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var video = await indexContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (!video.Owner.Address.IsTheSameAddress(address))
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Action.
            await indexContext.Videos.DeleteAsync(video);

            logger.AuthorDeletedVideo(id);
        }

        public async Task<VideoDto> FindByIdAsync(string id)
        {
            var video = await indexContext.Videos.FindOneAsync(v => v.Id == id);

            var lastValidManifest = video.GetLastValidManifest();
            VideoManifest? videoManifest = null;
            if (lastValidManifest is not null)
                videoManifest = await indexContext.VideoManifests.FindOneAsync(vm => vm.Id == lastValidManifest.Id);

            return new VideoDto(video, videoManifest);
        }

        public async Task<VideoDto> FindByManifestHashAsync(string hash)
        {
            var videoManifest = await indexContext.VideoManifests.FindOneAsync(vm => vm.ManifestHash.Hash == hash);

            var video = await indexContext.Videos.FindOneAsync(v => v.Id == videoManifest.Video.Id);

            return new VideoDto(video, videoManifest);
        }

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take)
        {
            // Get videos with valid manifest.
            var videos = await indexContext.Videos.QueryElementsAsync(elements =>
                elements.Where(i => i.VideoManifests.Any(k => k.IsValid == true))
                        .PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync());

            // Get manfinest info from video selected.
            var manifestIds = videos.Select(i => i.GetLastValidManifest()?.Id)
                                    .Where(i => !string.IsNullOrWhiteSpace(i));

            var manifests = await indexContext.VideoManifests.QueryElementsAsync(elements =>
                elements.Where(i => manifestIds.Contains(i.Id))
                        .ToListAsync());


            return videos.Select(v => new VideoDto(v, manifests.FirstOrDefault(i => i.Video.Id == v.Id)));
        }

        public async Task<ManifestStatusDto> ValidationStatusByHashAsync(string hash)
        {
            var manifest = await indexContext.VideoManifests.FindOneAsync(i => i.ManifestHash.Hash == hash);

            return new ManifestStatusDto(manifest);
        }

        public async Task<IEnumerable<ManifestStatusDto>> ValidationStatusByIdAsync(string videoId)
        {
            var manifest = await indexContext.Videos.FindOneAsync(i => i.Id == videoId);

            return manifest.VideoManifests
            .Select(i => new ManifestStatusDto(i));
        }

        public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string id, int page, int take) =>
            (await indexContext.Comments.QueryElementsAsync(elements =>
                elements.Where(c => c.Video.Id == id)
                        .PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync()))
                .Select(c => new CommentDto(c));

        public async Task<VideoManifestDto> UpdateAsync(string id, string newHash)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var video = await indexContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (!video.Owner.Address.IsTheSameAddress(address))
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Create videoManifest.
            var videoManifest = new VideoManifest(newHash, video);
            await indexContext.VideoManifests.CreateAsync(videoManifest);

            // Create Validation Manifest Task.
            backgroundJobClient.Create<MetadataVideoValidatorTask>(
                task => task.RunAsync(video.Id, newHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.UpdatedVideo(id, newHash);

            return new VideoManifestDto(videoManifest);
        }

        public async Task VoteVideAsync(string id, VoteValue value)
        {
            // Get data.
            var address = httpContextAccessor.HttpContext!.User.GetEtherAddress();
            var user = await indexContext.Users.FindOneAsync(u => u.Address == address);
            var video = await indexContext.Videos.FindOneAsync(id);

            // Remove prev votes of user on this content.
            var prevVotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Owner.Address == address && v.Video.Id == id)
                        .ToListAsync());
            foreach (var prevVote in prevVotes)
                await indexContext.Votes.DeleteAsync(prevVote);

            // Create new vote.
            var vote = new VideoVote(user, video, value);
            await indexContext.Votes.CreateAsync(vote);

            // Update counters on video.
            var totDownvotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.Id == id && v.Value == VoteValue.Down)
                        .LongCountAsync());
            var totUpvotes = await indexContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.Id == id && v.Value == VoteValue.Up)
                        .LongCountAsync());

            video.TotDownvotes = totDownvotes;
            video.TotUpvotes = totUpvotes;

            await indexContext.SaveChangesAsync();

            logger.VotedVideo(user.Id, id);
        }

    }
}
