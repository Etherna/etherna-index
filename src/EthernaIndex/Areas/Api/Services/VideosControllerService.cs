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
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Exceptions;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Etherna.MongODM.Core.Extensions;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class VideosControllerService : IVideosControllerService
    {
        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly ILogger<VideosControllerService> logger;
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;

        // Constructors.
        public VideosControllerService(
            IBackgroundJobClient backgroundJobClient,
            IIndexDbContext indexContext,
            ILogger<VideosControllerService> logger,
            ISharedDbContext sharedDbContext,
            IUserService userService)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.indexDbContext = indexContext;
            this.logger = logger;
            this.sharedDbContext = sharedDbContext;
            this.userService = userService;
        }

        // Methods.
        public async Task<string> CreateAsync(VideoCreateInput videoInput, ClaimsPrincipal userClaims)
        {
            var address = userClaims.GetEtherAddress();
            var (user, _) = await userService.FindUserAsync(address);

            var videoManifest = await indexDbContext.VideoManifests.TryFindOneAsync(c => c.Manifest.Hash == videoInput.ManifestHash);

            if (videoManifest is not null)
            {
                throw new DuplicatedManifestHashException(videoInput.ManifestHash);
            }

            // Create Video.
            var video = new Video(
                videoInput.EncryptionKey,
                videoInput.EncryptionType,
                user);

            await indexDbContext.Videos.CreateAsync(video);

            // Create video manifest.
            videoManifest = new VideoManifest(videoInput.ManifestHash);
            await indexDbContext.VideoManifests.CreateAsync(videoManifest);

            // Create Validation Manifest Task.
            backgroundJobClient.Create<MetadataVideoValidatorTask>(
                task => task.RunAsync(video.Id, videoInput.ManifestHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.CreatedVideo(user.Id, videoInput.ManifestHash);

            return video.Id;
        }

        public async Task<CommentDto> CreateCommentAsync(string id, string text, ClaimsPrincipal userClaims)
        {
            var address = userClaims.GetEtherAddress();
            var (user, userSharedInfo) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(id);

            var comment = new Comment(user, text, video);

            await indexDbContext.Comments.CreateAsync(comment);

            logger.CreatedCommentVideo(user.Id, id);

            return new CommentDto(comment, userSharedInfo);
        }

        public async Task DeleteAsync(string id, ClaimsPrincipal userClaims)
        {
            // Get data.
            var address = userClaims.GetEtherAddress();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var video = await indexDbContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (currentUser.Id != video.Owner.Id)
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Action.
            await indexDbContext.Videos.DeleteAsync(video);

            logger.AuthorDeletedVideo(id);
        }

        public async Task<VideoDto> FindByIdAsync(string id)
        {
            // Get Video.
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == id);

            // Get VideoManifest.
            var lastValidManifest = video.GetLastValidManifest();

            // Get User.
            var sharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

            return new VideoDto(video, lastValidManifest, sharedInfo);
        }

        public async Task<VideoDto> FindByManifestHashAsync(string hash)
        {
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.Manifest.Hash == hash);

            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

            return new VideoDto(video, videoManifest, ownerSharedInfo);
        }

        public async Task<IEnumerable<VideoDto>> GetLastUploadedVideosAsync(int page, int take)
        {
            // Get videos with valid manifest.
            var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
                elements.Where(v => v.VideoManifests.Any(vm => vm.IsValid == true))
                        .PaginateDescending(v => v.CreationDateTime, page, take)
                        .ToListAsync());

            // Get user info from video selected
            var videoDtos = new List<VideoDto>();
            foreach (var video in videos)
            {
                var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);
                videoDtos.Add(new VideoDto(
                    video,
                    video.GetLastValidManifest(),
                    ownerSharedInfo));
            }

            return videoDtos;
        }

        public async Task<ManifestStatusDto> GetValidationStatusByHashAsync(string hash)
        {
            var manifest = await indexDbContext.VideoManifests.FindOneAsync(i => i.Manifest.Hash == hash);

            return new ManifestStatusDto(manifest);
        }


        public async Task<IEnumerable<ManifestStatusDto>> GetValidationStatusByIdAsync(string videoId)
        {
            var manifest = await indexDbContext.Videos.FindOneAsync(i => i.Id == videoId);

            return manifest.VideoManifests
            .Select(i => new ManifestStatusDto(i));
        }

        public async Task<IEnumerable<CommentDto>> GetVideoCommentsAsync(string id, int page, int take)
        {
            var comments = await indexDbContext.Comments.QueryElementsAsync(elements =>
                elements.Where(c => c.Video.Id == id)
                        .PaginateDescending(c => c.CreationDateTime, page, take)
                        .ToListAsync());

            var commentDtos = new List<CommentDto>();
            foreach (var comment in comments)
            {
                var authorSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(comment.Author.Id);
                commentDtos.Add(new CommentDto(comment, authorSharedInfo));
            }

            return commentDtos;
        }

        public async Task ReportVideoAsync(string videoId, string description, ClaimsPrincipal userClaims)
        {
            // Get video.
            var video = await indexDbContext.Videos.FindOneAsync(videoId);

            // Get user info.
            var address = userClaims.GetEtherAddress();
            var (user, userSharedInfo) = await userService.FindUserAsync(address);

            // Add or Update UnsuitableVideoReport.
            var videoReport = await indexDbContext.UnsuitableVideoReports
                .TryFindOneAsync(v => v.Video.Id == video.Id &&
                                      v.ReporterAuthor.Id == userSharedInfo.Id);

            if (videoReport is null)
            {
                // Create.
                var videoReported = new UnsuitableVideoReport(video, user, description);
                await indexDbContext.UnsuitableVideoReports.CreateAsync(videoReported);
            }
            else
            {
                // Edit.
                videoReport.ChangeDescription(description);
                await indexDbContext.SaveChangesAsync();
            }
        }

        public async Task<VideoManifestDto> UpdateAsync(string id, string newHash, ClaimsPrincipal userClaims)
        {
            // Get data.
            var address = userClaims.GetEtherAddress();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var video = await indexDbContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (video.Owner.Id != currentUser.Id)
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Create videoManifest.
            var videoManifest = new VideoManifest(newHash);
            await indexDbContext.VideoManifests.CreateAsync(videoManifest);

            // Create Validation Manifest Task.
            backgroundJobClient.Create<MetadataVideoValidatorTask>(
                task => task.RunAsync(video.Id, newHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.UpdatedVideo(id, newHash);

            return new VideoManifestDto(videoManifest);
        }

        public async Task VoteVideAsync(string id, VoteValue value, ClaimsPrincipal userClaims)
        {
            // Get data.
            var address = userClaims.GetEtherAddress();
            var (user, userSharedInfo) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(id);

            // Remove prev votes of user on this content.
            var prevVotes = await indexDbContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Owner.Id == user.Id && v.Video.Id == id)
                        .ToListAsync());
            foreach (var prevVote in prevVotes)
                await indexDbContext.Votes.DeleteAsync(prevVote);

            // Create new vote.
            var vote = new VideoVote(user, video, value);
            await indexDbContext.Votes.CreateAsync(vote);

            // Update counters on video.
            var totDownvotes = await indexDbContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.Id == id && v.Value == VoteValue.Down)
                        .LongCountAsync());
            var totUpvotes = await indexDbContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Video.Id == id && v.Value == VoteValue.Up)
                        .LongCountAsync());

            video.TotDownvotes = totDownvotes;
            video.TotUpvotes = totUpvotes;

            await indexDbContext.SaveChangesAsync();

            logger.VotedVideo(user.Id, id);
        }
    }
}
