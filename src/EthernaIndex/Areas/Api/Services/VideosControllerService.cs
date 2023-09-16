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

using Etherna.Authentication;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Exceptions;
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal sealed class VideosControllerService : IVideosControllerService
    {
        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly IIndexDbContext indexDbContext;
        private readonly ILogger<VideosControllerService> logger;
        private readonly ISharedDbContext sharedDbContext;
        private readonly IUserService userService;
        private readonly IVideoService videoService;

        // Constructors.
        public VideosControllerService(
            IBackgroundJobClient backgroundJobClient,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            IIndexDbContext indexDbContext,
            ILogger<VideosControllerService> logger,
            ISharedDbContext sharedDbContext,
            IUserService userService,
            IVideoService videoService)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.ethernaOidcClient = ethernaOidcClient;
            this.indexDbContext = indexDbContext;
            this.logger = logger;
            this.sharedDbContext = sharedDbContext;
            this.userService = userService;
            this.videoService = videoService;
        }

        // Methods.
        public async Task AuthorDeleteAsync(string id)
        {
            // Get data.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var video = await indexDbContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (currentUser.Id != video.Owner.Id)
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Action.
            await videoService.DeleteVideoAsync(video);

            logger.AuthorDeleteVideo(id);
        }

        public async Task<string> CreateAsync(VideoCreateInput videoInput)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var videoManifest = await indexDbContext.VideoManifests.TryFindOneAsync(c => c.Manifest.Hash == videoInput.ManifestHash);

            if (videoManifest is not null)
            {
                // Act as an idempotent call if video and creator are the same.
                var existingVideo = await indexDbContext.Videos
                    .TryFindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

                if (existingVideo is null ||
                    existingVideo.Owner.Id != currentUser.Id)
                    throw new DuplicatedManifestHashException(videoInput.ManifestHash);

                return existingVideo.Id;
            }

            // Create Video.
            var video = new Video(currentUser);

            await indexDbContext.Videos.CreateAsync(video);

            // Create video manifest.
            videoManifest = new VideoManifest(videoInput.ManifestHash);
            await indexDbContext.VideoManifests.CreateAsync(videoManifest);

            // Add manifest to video.
            video = await indexDbContext.Videos.FindOneAsync(video.Id); //find again because needs to be a proxy for update (see: MODM-83)
            video.AddManifest(videoManifest);
            await indexDbContext.SaveChangesAsync();

            // Create Validation Manifest Task.
            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, videoInput.ManifestHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.VideoCreated(currentUser.Id, videoInput.ManifestHash);

            return video.Id;
        }

        public async Task<Comment2Dto> CreateCommentAsync(string id, string text)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, userSharedInfo) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(id);

            var comment = new Comment(user, text, video);

            await indexDbContext.Comments.CreateAsync(comment);

            logger.CreateVideoComment(user.Id, id);

            return new Comment2Dto(comment, userSharedInfo);
        }

        public async Task<Video2Dto> FindByIdAsync(string id)
        {
            // Get Video.
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == id);

            // Get VideoManifest.
            var lastValidManifest = video.LastValidManifest;

            // Get Owner User.
            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

            // Get Current User and Vote
            VideoVote? currentUserVideoVote = null;
            var etherAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            if (etherAddress is not null)
            {
                var (currentUser, _) = await userService.FindUserAsync(etherAddress);
                currentUserVideoVote = await indexDbContext.Votes.TryFindOneAsync(v => v.Video.Id == id &&
                                                                                       v.Owner.Id == currentUser.Id);
            }

            logger.FindVideoById(id);

            return new Video2Dto(video, lastValidManifest, ownerSharedInfo, currentUserVideoVote);
        }

        public async Task<Video2GenericManifestDto> FindByManifestHashAsync(string hash)
        {
            // Get Video.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.Manifest.Hash == hash);

            // Get VideoManifest.
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            // Get Owner User.
            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

            // Get Current User and Vote
            VideoVote? currentUserVideoVote = null;
            var etherAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            if (etherAddress is not null)
            {
                var (currentUser, _) = await userService.FindUserAsync(etherAddress);
                currentUserVideoVote = await indexDbContext.Votes.TryFindOneAsync(v => v.Video.Id == video.Id &&
                                                                                       v.Owner.Id == currentUser.Id);
            }

            logger.FindManifestByHash(hash);

            return new Video2GenericManifestDto(video, videoManifest, ownerSharedInfo, currentUserVideoVote);
        }

        public async Task<IEnumerable<VideoManifestStatusDto>> GetBulkValidationStatusByHashesAsync(IEnumerable<string> manifestHashes)
        {
            var manifests = await indexDbContext.VideoManifests.QueryElementsAsync(
                elements => elements.Where(m => manifestHashes.Contains(m.Manifest.Hash))
                                    .ToListAsync());

            logger.GetBulkVideoManifestValidationStatusByHashes(manifestHashes);

            return manifests.Select(m => new VideoManifestStatusDto(m));
        }

        public async Task<IEnumerable<VideoStatusDto>> GetBulkValidationStatusByIdsAsync(IEnumerable<string> videoIds)
        {
            var videos = await indexDbContext.Videos.QueryElementsAsync(
                elements => elements.Where(v => videoIds.Contains(v.Id))
                                    .ToListAsync());

            logger.GetBulkVideoValidationStatusByIds(videoIds);

            return videos.Select(v => new VideoStatusDto(v));
        }

        public async Task<PaginatedEnumerableDto<VideoPreviewDto>> GetLastUploadedVideosAsync(int page, int take)
        {
            // Get videos with valid manifest.
            var paginatedVideos = await indexDbContext.Videos.QueryPaginatedElementsAsync(
                elements => elements.Where(v => v.LastValidManifest != null),
                v => v.CreationDateTime,
                page,
                take,
                true);

            // Get user info from video selected
            var videoPreviews = new List<VideoPreviewDto>();
            foreach (var video in paginatedVideos.Elements)
            {
                var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);
                videoPreviews.Add(new VideoPreviewDto(
                    video,
                    ownerSharedInfo));
            }

            logger.GetLastUploadedVideos(page, take);

            return new PaginatedEnumerableDto<VideoPreviewDto>(
                paginatedVideos.CurrentPage,
                videoPreviews,
                paginatedVideos.PageSize,
                paginatedVideos.TotalElements);
        }

        public async Task<VideoManifestStatusDto> GetValidationStatusByHashAsync(string hash)
        {
            var manifest = await indexDbContext.VideoManifests.FindOneAsync(i => i.Manifest.Hash == hash);

            logger.GetVideoManifestValidationStatusByHash(hash);

            return new VideoManifestStatusDto(manifest);
        }

        public async Task<VideoStatusDto> GetValidationStatusByIdAsync(string videoId)
        {
            var video = await indexDbContext.Videos.FindOneAsync(i => i.Id == videoId);

            logger.GetVideoValidationStatusById(videoId);

            return new VideoStatusDto(video);
        }

        public async Task<PaginatedEnumerableDto<Comment2Dto>> GetVideoCommentsAsync(string id, int page, int take)
        {
            var paginatedComments = await indexDbContext.Comments.QueryPaginatedElementsAsync(
                elements => elements.Where(c => c.Video.Id == id),
                c => c.CreationDateTime,
                page,
                take,
                true);

            var commentDtos = new List<Comment2Dto>();
            foreach (var comment in paginatedComments.Elements)
            {
                var author = await indexDbContext.Users.FindOneAsync(comment.Author.Id);
                var authorSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(author.SharedInfoId);
                commentDtos.Add(new Comment2Dto(comment, authorSharedInfo));
            }

            logger.GetVideoComments(id, page, take);

            return new PaginatedEnumerableDto<Comment2Dto>(
                paginatedComments.CurrentPage,
                commentDtos,
                paginatedComments.PageSize,
                paginatedComments.TotalElements);
        }

        public async Task ReportVideoAsync(string videoId, string manifestHash, string description)
        {
            // Get video and manifest.
            var video = await indexDbContext.Videos.FindOneAsync(videoId);
            var manifest = video.VideoManifests.First(m => m.Manifest.Hash == manifestHash);

            // Get user info.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, userSharedInfo) = await userService.FindUserAsync(address);

            // Add or Update UnsuitableVideoReport.
            var videoReport = await indexDbContext.UnsuitableVideoReports
                .TryFindOneAsync(v => v.VideoManifest.Id == manifest.Id &&
                                      v.ReporterAuthor.SharedInfoId == userSharedInfo.Id);

            if (videoReport is null)
            {
                // Create.
                var videoReported = new UnsuitableVideoReport(video, manifest, user, description);
                await indexDbContext.UnsuitableVideoReports.CreateAsync(videoReported);
                logger.CreateVideoReport(videoId, manifestHash);
            }
            else
            {
                // Edit.
                videoReport.ChangeDescription(description);
                await indexDbContext.SaveChangesAsync();

                logger.ChangeVideoReportDescription(videoId, manifestHash);
            }
        }

        public async Task<VideoManifest2Dto> UpdateAsync(string id, string newHash)
        {
            var videoManifest = await UpdateCommonAsync(id, newHash);
            return new VideoManifest2Dto(videoManifest);
        }

        public async Task UpdateCommentAsync(string commentId, string text)
        {
            // Get data.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var comment = await indexDbContext.Comments.FindOneAsync(commentId);
            if (comment.Author.Id != currentUser.Id)
                throw new UnauthorizedAccessException("Only the owner of comment can update the content");

            comment.EditByAuthor(text);
            
            await indexDbContext.SaveChangesAsync();
            logger.UpdatedComment(commentId);
        }

        public async Task VoteVideAsync(string id, VoteValue value)
        {
            // Get data.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, _) = await userService.FindUserAsync(address);
            var video = await indexDbContext.Videos.FindOneAsync(id);

            // Remove prev votes of user on this content.
            var prevVotes = await indexDbContext.Votes.QueryElementsAsync(elements =>
                elements.Where(v => v.Owner.Id == user.Id && v.Video.Id == id)
                        .ToListAsync());
            foreach (var prevVote in prevVotes)
                await indexDbContext.Votes.DeleteAsync(prevVote);

            // Create new vote.
            if (value != VoteValue.Neutral)
            {
                var vote = new VideoVote(user, video, value);
                await indexDbContext.Votes.CreateAsync(vote);
            }

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

            logger.VideoVoted(user.Id, id);
        }

        //deprecated
        [Obsolete("Used only for API backwards compatibility")]
        public async Task<VideoDto> FindByIdAsync_old(string id)
        {
            // Get Video.
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == id);

            // Get VideoManifest.
            var lastValidManifest = video.LastValidManifest;

            // Get Owner User.
            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

            // Get Current User and Vote
            VideoVote? currentUserVideoVote = null;
            var etherAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            if (etherAddress is not null)
            {
                var (currentUser, _) = await userService.FindUserAsync(etherAddress);
                currentUserVideoVote = await indexDbContext.Votes.TryFindOneAsync(v => v.Video.Id == id &&
                                                                                       v.Owner.Id == currentUser.Id);
            }

            logger.FindVideoById(id);

            return new VideoDto(video, lastValidManifest, ownerSharedInfo, currentUserVideoVote);
        }

        [Obsolete("Used only for API backwards compatibility")]
        public async Task<VideoDto> FindByManifestHashAsync_old(string hash)
        {
            // Get Video.
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.Manifest.Hash == hash);

            // Get VideoManifest.
            var video = await indexDbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

            // Get Owner User.
            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

            // Get Current User and Vote
            VideoVote? currentUserVideoVote = null;
            var etherAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
            if (etherAddress is not null)
            {
                var (currentUser, _) = await userService.FindUserAsync(etherAddress);
                currentUserVideoVote = await indexDbContext.Votes.TryFindOneAsync(v => v.Video.Id == video.Id &&
                                                                                       v.Owner.Id == currentUser.Id);
            }

            logger.FindManifestByHash(hash);

            return new VideoDto(video, videoManifest, ownerSharedInfo, currentUserVideoVote);
        }

        [Obsolete("Used only for API backwards compatibility")]
        public async Task<PaginatedEnumerableDto<VideoDto>> GetLastUploadedVideosAsync_old(int page, int take)
        {
            // Get videos with valid manifest.
            var paginatedVideos = await indexDbContext.Videos.QueryPaginatedElementsAsync(
                elements => elements.Where(v => v.LastValidManifest != null),
                v => v.CreationDateTime,
                page,
                take,
                true);

            // Get user info from video selected
            var videoDtos = new List<VideoDto>();
            foreach (var video in paginatedVideos.Elements)
            {
                var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);
                videoDtos.Add(new VideoDto(
                    video,
                    video.LastValidManifest,
                    ownerSharedInfo,
                    null));
            }

            logger.GetLastUploadedVideos(page, take);

            return new PaginatedEnumerableDto<VideoDto>(
                paginatedVideos.CurrentPage,
                videoDtos,
                paginatedVideos.PageSize,
                paginatedVideos.TotalElements);
        }

        [Obsolete("Used only for API backwards compatibility")]
        public async Task<PaginatedEnumerableDto<CommentDto>> GetVideoCommentsAsync_old(string id, int page, int take)
        {
            var paginatedComments = await GetVideoCommentsAsync(id, page, take);

            return new PaginatedEnumerableDto<CommentDto>(
                paginatedComments.CurrentPage,
                paginatedComments.Elements.Select(c => new CommentDto(c)),
                paginatedComments.PageSize,
                paginatedComments.TotalElements);
        }

        [Obsolete("Used only for API backwards compatibility")]
        public async Task<VideoManifestDto> UpdateAsync_old(string id, string newHash)
        {
            var videoManifest = await UpdateCommonAsync(id, newHash);
            return new VideoManifestDto(videoManifest);
        }

        // Helpers.
        private async Task<VideoManifest> UpdateCommonAsync(string id, string newHash)
        {
            // Get data.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var video = await indexDbContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (video.Owner.Id != currentUser.Id)
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Create videoManifest.
            var videoManifest = new VideoManifest(newHash);
            await indexDbContext.VideoManifests.CreateAsync(videoManifest);

            // Add manifest to video.
            video.AddManifest(videoManifest);
            await indexDbContext.SaveChangesAsync();

            // Create Validation Manifest Task.
            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, newHash),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.UpdatedVideo(id, newHash);

            return videoManifest;
        }
    }
}
