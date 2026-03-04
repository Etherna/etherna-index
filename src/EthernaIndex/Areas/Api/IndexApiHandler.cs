// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.Authentication;
using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Exceptions;
using Etherna.EthernaIndex.Services.Extensions;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.MongoDB.Driver.Linq;
using Hangfire;
using Hangfire.States;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api
{
    internal sealed class IndexApiHandler(
        IAuthorizationService authorizationService,
        IBackgroundJobClient backgroundJobClient,
        IIndexDbContext dbContext,
        IElasticSearchService elasticSearchService,
        IEthernaOpenIdConnectClient ethernaOidcClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<IndexApiHandler> logger,
        ISharedDbContext sharedDbContext,
        IUserService userService,
        IVideoService videoService)
        : IIndexApiHandler
    {
        // Methods.
        public Task<IResult> AuthorDeleteVideoAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get data.
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (currentUser, _) = await userService.FindUserAsync(address);

                var video = await dbContext.Videos.FindOneAsync(id);

                // Verify authz.
                if (currentUser.Id != video.Owner.Id)
                    throw new UnauthorizedAccessException("User is not owner of the video");

                // Action.
                await videoService.DeleteVideoAsync(video);

                logger.AuthorDeleteVideo(id);

                return Results.Ok();
            });

        public Task<IResult> CreateCommentAsync(string id, string text) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (user, userSharedInfo) = await userService.FindUserAsync(address);
                var video = await dbContext.Videos.FindOneAsync(id);

                var comment = new Comment(user, text, video);

                await dbContext.Comments.CreateAsync(comment);

                logger.CreateVideoComment(user.Id, id);

                return Results.Json(
                    new Comment2Dto(comment, userSharedInfo),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> CreateCommentAsync_old(string id, string text) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (user, userSharedInfo) = await userService.FindUserAsync(address);
                var video = await dbContext.Videos.FindOneAsync(id);

                var comment = new Comment(user, text, video);

                await dbContext.Comments.CreateAsync(comment);

                logger.CreateVideoComment(user.Id, id);

                return Results.Json(
                    new CommentDto(comment, userSharedInfo),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> CreateVideoAsync(SwarmReference manifestReference, PostageBatchId? batchId) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (currentUser, _) = await userService.FindUserAsync(address);

                var videoManifest = await dbContext.VideoManifests.TryFindOneAsync(c => c.ManifestReference == manifestReference);

                if (videoManifest is not null)
                {
                    // Act as an idempotent call if video and creator are the same.
                    var existingVideo = await dbContext.Videos
                        .TryFindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

                    if (existingVideo is null ||
                        existingVideo.Owner.Id != currentUser.Id)
                        throw new DuplicatedManifestReferenceException(manifestReference);

                    return Results.Json(
                        existingVideo.Id,
                        CommonConsts.IndexV03JsonSerializerOptions);
                }

                // Create Video.
                var video = new Video(currentUser, batchId);

                await dbContext.Videos.CreateAsync(video);

                // Create video manifest.
                videoManifest = new VideoManifest(manifestReference);
                await dbContext.VideoManifests.CreateAsync(videoManifest);

                // Add manifest to video.
                video = await dbContext.Videos.FindOneAsync(video.Id); //find again because needs to be a proxy for update (see: MODM-83)
                video.AddManifest(videoManifest);
                await dbContext.SaveChangesAsync();

                // Create Validation Manifest Task.
                backgroundJobClient.Create<IVideoManifestValidatorTask>(
                    task => task.RunAsync(video.Id, manifestReference.ToString()),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

                logger.VideoCreated(currentUser.Id, video.Id);

                return Results.Json(
                    video.Id,
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> DeleteOwnedCommentAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get data.
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (currentUser, _) = await userService.FindUserAsync(address);

                var comment = await dbContext.Comments.FindOneAsync(id);

                // Verify authorization.
                if (comment.Author.Id != currentUser.Id)
                    throw new UnauthorizedAccessException("User is not owner of the comment");

                // Action.
                comment.SetAsDeletedByAuthor();

                await dbContext.SaveChangesAsync();

                logger.OwnerDeleteVideoComment(id);
                
                return Results.Ok();
            });

        public Task<IResult> FindUserByEthAddressAsync(EthAddress address) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var (user, sharedInfo) = await userService.FindUserAsync(address);

                logger.FindUserByAddress(address);

                return Results.Json(
                    new UserDto(user, sharedInfo),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> FindVideoByIdAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get Video.
                var video = await dbContext.Videos.FindOneAsync(v => v.Id == id);

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
                    currentUserVideoVote = await dbContext.Votes.TryFindOneAsync(v => v.Video.Id == id &&
                        v.Owner.Id == currentUser.Id);
                }

                logger.FindVideoById(id);

                return Results.Json(
                    new Video2Dto(video, lastValidManifest, ownerSharedInfo, currentUserVideoVote),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> FindVideoByIdAsync_old(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get Video.
                var video = await dbContext.Videos.FindOneAsync(v => v.Id == id);

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
                    currentUserVideoVote = await dbContext.Votes.TryFindOneAsync(v => v.Video.Id == id &&
                        v.Owner.Id == currentUser.Id);
                }

                logger.FindVideoById(id);

                return Results.Json(
                    new VideoDto(video, lastValidManifest, ownerSharedInfo, currentUserVideoVote),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> FindVideoByManifestReferenceAsync(SwarmReference reference) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get VideoManifest.
                var videoManifest = await dbContext.VideoManifests.FindOneAsync(vm => vm.ManifestReference == reference);

                // Get Video.
                var video = await dbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

                // Get Owner User.
                var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

                // Get Current User and Vote
                VideoVote? currentUserVideoVote = null;
                var etherAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
                if (etherAddress is not null)
                {
                    var (currentUser, _) = await userService.FindUserAsync(etherAddress);
                    currentUserVideoVote = await dbContext.Votes.TryFindOneAsync(v => v.Video.Id == video.Id &&
                        v.Owner.Id == currentUser.Id);
                }

                logger.FindManifestByReference(reference);

                return Results.Json(
                    new Video2Dto(video, videoManifest, ownerSharedInfo, currentUserVideoVote),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> FindVideoByManifestReferenceAsync_old(SwarmReference reference) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get Video.
                var videoManifest = await dbContext.VideoManifests.FindOneAsync(vm => vm.ManifestReference == reference);

                // Get VideoManifest.
                var video = await dbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

                // Get Owner User.
                var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);

                // Get Current User and Vote
                VideoVote? currentUserVideoVote = null;
                var etherAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
                if (etherAddress is not null)
                {
                    var (currentUser, _) = await userService.FindUserAsync(etherAddress);
                    currentUserVideoVote = await dbContext.Votes.TryFindOneAsync(v => v.Video.Id == video.Id &&
                        v.Owner.Id == currentUser.Id);
                }

                logger.FindManifestByReference(reference);

                return Results.Json(
                    new VideoDto(video, videoManifest, ownerSharedInfo, currentUserVideoVote),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> ForceVideoManifestValidationAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var video = await dbContext.Videos.FindOneAsync(v => v.Id == id);

                foreach (var manifest in video.VideoManifests)
                {
                    backgroundJobClient.Create<IVideoManifestValidatorTask>(
                        task => task.RunAsync(video.Id, manifest.ManifestReference.ToString()),
                        new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));
                }

                logger.ForcedVideoManifestsValidation(
                    await ethernaOidcClient.GetClientIdAsync(),
                    video.Id,
                    video.VideoManifests.Select(m => m.ManifestReference));
                
                return Results.Ok();
            });

        public Task<IResult> ForceVideoManifestValidationAsync(SwarmReference reference) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videoManifest = await dbContext.VideoManifests.FindOneAsync(c => c.ManifestReference == reference);
                var video = await dbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == videoManifest.Id));

                backgroundJobClient.Create<IVideoManifestValidatorTask>(
                    task => task.RunAsync(video.Id, reference.ToString()),
                    new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

                logger.ForcedVideoManifestsValidation(await ethernaOidcClient.GetClientIdAsync(), video.Id, [reference]);
                
                return Results.Ok();
            });

        public Task<IResult> GetCurrentUserAsync() =>
            ExceptionHandler.RunAsync(async () =>
            {
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (user, sharedInfo) = await userService.FindUserAsync(address);

                var isSuperModeratorResult = await authorizationService.AuthorizeAsync(
                    httpContextAccessor.HttpContext!.User, CommonConsts.RequireSuperModeratorRolePolicy);

                logger.GetCurrentUser(address);

                return Results.Json(
                    new CurrentUserDto(user, sharedInfo, isSuperModeratorResult.Succeeded),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetBulkVideoValidationStatusByIdsAsync(IEnumerable<string> ids) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videos = await dbContext.Videos.QueryElementsAsync(
                    elements => elements.Where(v => ids.Contains(v.Id))
                        .ToListAsync());

                logger.GetBulkVideoValidationStatusByIds(ids);

                return Results.Json(
                    videos.SelectMany(v => v.VideoManifests.Select(vm => new VideoManifestStatusDto(v, vm))),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetBulkVideoValidationStatusByIdsAsync_old(IEnumerable<string> ids) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videos = await dbContext.Videos.QueryElementsAsync(
                    elements => elements.Where(v => ids.Contains(v.Id))
                        .ToListAsync());

                logger.GetBulkVideoValidationStatusByIds(ids);

                return Results.Json(
                    videos.Select(v => new VideoStatusDto(v)),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetBulkVideoValidationStatusByReferencesAsync(IEnumerable<SwarmReference> references) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videoManifests = await dbContext.VideoManifests.QueryElementsAsync(
                    elements => elements.Where(m => references.Contains(m.ManifestReference))
                        .ToListAsync());
                var videoManifestsIds = videoManifests.Select(vm => vm.Id);
                var videos = await dbContext.Videos.QueryElementsAsync(
                    elements => elements.Where(v => v.VideoManifests.Any(vm => videoManifestsIds.Contains(vm.Id)))
                        .ToListAsync());

                logger.GetBulkVideoManifestValidationStatusByReferences(references);

                return Results.Json(
                    videoManifests.Select(m => new VideoManifestStatusDto(
                        videos.First(v => v.VideoManifests.Any(vm => vm.Id == m.Id)),
                        m)),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetIndexParameters() =>
            ExceptionHandler.RunAsync(() =>
                Task.FromResult(Results.Json(new SystemParametersDto(), CommonConsts.IndexV03JsonSerializerOptions)));

        public Task<IResult> GetLastUploadedVideosAsync(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get videos with valid manifest.
                var paginatedVideos = await dbContext.Videos.QueryPaginatedElementsAsync(
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

                return Results.Json(
                    new PaginatedEnumerableDto<VideoPreviewDto>(
                        paginatedVideos.CurrentPage,
                        videoPreviews,
                        paginatedVideos.PageSize,
                        paginatedVideos.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetLastUploadedVideosAsync_old(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get videos with valid manifest.
                var paginatedVideos = await dbContext.Videos.QueryPaginatedElementsAsync(
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

                return Results.Json(
                    videoDtos,
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetLastUploadedVideosAsync_old2(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get videos with valid manifest.
                var paginatedVideos = await dbContext.Videos.QueryPaginatedElementsAsync(
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

                return Results.Json(
                    new PaginatedEnumerableDto<VideoDto>(
                        paginatedVideos.CurrentPage,
                        videoDtos,
                        paginatedVideos.PageSize,
                        paginatedVideos.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetUsersAsync(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var paginatedUsers = await dbContext.Users.QueryPaginatedElementsAsync(
                    elements => elements,
                    u => u.CreationDateTime,
                    page,
                    take,
                    true);

                var userDtos = new List<UserDto>();
                foreach (var user in paginatedUsers.Elements)
                {
                    var sharedInfo = await sharedDbContext.UsersInfo.TryFindOneAsync(user.SharedInfoId);
                    userDtos.Add(new UserDto(user, sharedInfo));
                }

                logger.GetUserListPaginated(page, take);

                return Results.Json(
                    new PaginatedEnumerableDto<UserDto>(
                        paginatedUsers.CurrentPage,
                        userDtos,
                        paginatedUsers.PageSize,
                        paginatedUsers.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetUsersAsync_old(int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var paginatedUsers = await dbContext.Users.QueryPaginatedElementsAsync(
                    elements => elements,
                    u => u.CreationDateTime,
                    page,
                    take,
                    true);

                var userDtos = new List<UserDto>();
                foreach (var user in paginatedUsers.Elements)
                {
                    var sharedInfo = await sharedDbContext.UsersInfo.TryFindOneAsync(user.SharedInfoId);
                    userDtos.Add(new UserDto(user, sharedInfo));
                }

                logger.GetUserListPaginated(page, take);

                return Results.Json(
                    userDtos,
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetVideoCommentsAsync(string id, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
                Results.Json(
                    await GetVideoCommentsHelperAsync(id, page, take),
                    CommonConsts.IndexV03JsonSerializerOptions));

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetVideoCommentsAsync_old(string id, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var paginatedComments = await GetVideoCommentsHelperAsync(id, page, take);

                return Results.Json(
                    paginatedComments.Elements.Select(c => new CommentDto(c)),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetVideoCommentsAsync_old2(string id, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var paginatedComments = await GetVideoCommentsHelperAsync(id, page, take);

                return Results.Json(
                    new PaginatedEnumerableDto<CommentDto>(
                        paginatedComments.CurrentPage,
                        paginatedComments.Elements.Select(c => new CommentDto(c)),
                        paginatedComments.PageSize,
                        paginatedComments.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetVideosAsync(EthAddress address, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var currentUserAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
                var requestByVideoOwner = currentUserAddress != null &&
                                          address == currentUserAddress;

                var (user, sharedInfo) = await userService.FindUserAsync(address);
                var paginatedVideos = await dbContext.Videos.QueryPaginatedElementsAsync(
                    elements => elements.Where(v => v.Owner.Id == user.Id)
                        .Where(v => requestByVideoOwner || v.LastValidManifest != null),
                    v => v.CreationDateTime,
                    page,
                    take,
                    true);

                logger.GetUserVideosPaginated(address, page, take);

                return Results.Json(
                    new PaginatedEnumerableDto<Video2Dto>(
                        paginatedVideos.CurrentPage,
                        paginatedVideos.Elements.Select(v => new Video2Dto(v, v.LastValidManifest, sharedInfo, null)),
                        paginatedVideos.PageSize,
                        paginatedVideos.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetVideosAsync_old(EthAddress address, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var currentUserAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
                var requestByVideoOwner = currentUserAddress != null &&
                                          address == currentUserAddress;

                var (user, sharedInfo) = await userService.FindUserAsync(address);
                var paginatedVideos = await dbContext.Videos.QueryPaginatedElementsAsync(
                    elements => elements.Where(v => v.Owner.Id == user.Id)
                        .Where(v => requestByVideoOwner || v.LastValidManifest != null),
                    v => v.CreationDateTime,
                    page,
                    take,
                    true);

                logger.GetUserVideosPaginated(address, page, take);

                return Results.Json(
                    paginatedVideos.Elements.Select(v => new VideoDto(v, v.LastValidManifest, sharedInfo, null)),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetVideosAsync_old2(EthAddress address, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var currentUserAddress = await ethernaOidcClient.TryGetEtherAddressAsync();
                var requestByVideoOwner = currentUserAddress != null &&
                                          address == currentUserAddress;

                var (user, sharedInfo) = await userService.FindUserAsync(address);
                var paginatedVideos = await dbContext.Videos.QueryPaginatedElementsAsync(
                    elements => elements.Where(v => v.Owner.Id == user.Id)
                        .Where(v => requestByVideoOwner || v.LastValidManifest != null),
                    v => v.CreationDateTime,
                    page,
                    take,
                    true);

                logger.GetUserVideosPaginated(address, page, take);

                return Results.Json(
                    new PaginatedEnumerableDto<VideoDto>(
                        paginatedVideos.CurrentPage,
                        paginatedVideos.Elements.Select(v => new VideoDto(v, v.LastValidManifest, sharedInfo, null)),
                        paginatedVideos.PageSize,
                        paginatedVideos.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetVideoValidationStatusByIdAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var video = await dbContext.Videos.FindOneAsync(i => i.Id == id);

                logger.GetVideoValidationStatusById(id);

                return Results.Json(
                    video.VideoManifests.Select(vm => new VideoManifestStatusDto(video, vm)),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> GetVideoValidationStatusByReferenceAsync(SwarmReference reference) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var manifest = await dbContext.VideoManifests.FindOneAsync(i => i.ManifestReference == reference);
                var video = await dbContext.Videos.FindOneAsync(v => v.VideoManifests.Any(vm => vm.Id == manifest.Id));

                logger.GetVideoManifestValidationStatusByReference(reference);

                return Results.Json(
                    new VideoManifestStatusDto(video, manifest),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetVideoValidationStatusByIdAsync_old(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var video = await dbContext.Videos.FindOneAsync(i => i.Id == id);

                logger.GetVideoValidationStatusById(id);

                return Results.Json(
                    video.VideoManifests.Select(vm => new VideoManifestStatusDto(video, vm)),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> GetVideoValidationStatusByIdAsync_old2(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var video = await dbContext.Videos.FindOneAsync(i => i.Id == id);

                logger.GetVideoValidationStatusById(id);

                return Results.Json(
                    new VideoStatusDto(video),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> ModerateCommentAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var comment = await dbContext.Comments.FindOneAsync(id);
                comment.SetAsDeletedByModerator();
                await dbContext.SaveChangesAsync();

                logger.ModerateComment(id);
                
                return Results.Ok();
            });

        public Task<IResult> ModerateVideoAsync(string id) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var video = await dbContext.Videos.FindOneAsync(id);
                await videoService.ModerateUnsuitableVideoAsync(video);

                logger.ModerateVideo(id);
                
                return Results.Ok();
            });

        public Task<IResult> RebuildElasticIndexes() =>
            ExceptionHandler.RunAsync(() =>
            {
                backgroundJobClient.Enqueue<IRebuildElasticIndexesTask>(t => t.RunAsync());
                return Task.FromResult(Results.Ok());
            });

        public Task<IResult> ReportVideoAsync(string id, SwarmReference reference, string description) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get video and manifest.
                var video = await dbContext.Videos.FindOneAsync(id);
                var manifest = video.VideoManifests.First(m => m.ManifestReference == reference);

                // Get user info.
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (user, userSharedInfo) = await userService.FindUserAsync(address);

                // Add or Update UnsuitableVideoReport.
                var videoReport = await dbContext.UnsuitableVideoReports
                    .TryFindOneAsync(v => v.VideoManifest.Id == manifest.Id &&
                                          v.ReporterAuthor.SharedInfoId == userSharedInfo.Id);

                if (videoReport is null)
                {
                    // Create.
                    var videoReported = new UnsuitableVideoReport(video, manifest, user, description);
                    await dbContext.UnsuitableVideoReports.CreateAsync(videoReported);
                    logger.CreateVideoReport(id, reference);
                }
                else
                {
                    // Edit.
                    videoReport.ChangeDescription(description);
                    await dbContext.SaveChangesAsync();

                    logger.ChangeVideoReportDescription(id, reference);
                }

                return Results.Ok();
            });

        public Task<IResult> SearchVideoAsync(string query, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var paginatedVideoDocuments = await elasticSearchService.SearchVideoAsync(query, page, take);

                // Get user info from video selected.
                var cacheSharedInfos = new Dictionary<string, UserSharedInfo>();
                var videoDtos = new List<VideoPreviewDto>();
                foreach (var videoDocument in paginatedVideoDocuments.Results)
                {
                    // Get shared info.
                    if (!cacheSharedInfos.ContainsKey(videoDocument.OwnerSharedInfoId))
                        cacheSharedInfos[videoDocument.OwnerSharedInfoId] = await sharedDbContext.UsersInfo.FindOneAsync(videoDocument.OwnerSharedInfoId);

                    // Create video dto.
                    videoDtos.Add(new VideoPreviewDto(
                        videoDocument,
                        cacheSharedInfos[videoDocument.OwnerSharedInfoId]));
                }

                return Results.Json(
                    new PaginatedEnumerableDto<VideoPreviewDto>(
                        page,
                        videoDtos,
                        take,
                        paginatedVideoDocuments.TotalElements),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> SearchVideoAsync_old(string query, int page, int take) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videoDocuments = await elasticSearchService.SearchVideoAsync(query, page, take);

                // Get user info from video selected.
                var cacheSharedInfos = new Dictionary<string, UserSharedInfo>();
                var videoDtos = new List<VideoDto>();
                foreach (var videoDocument in videoDocuments.Results)
                {
                    // Get shared info.
                    if (!cacheSharedInfos.TryGetValue(videoDocument.OwnerSharedInfoId, out var sharedInfo))
                    {
                        sharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(videoDocument.OwnerSharedInfoId);
                        cacheSharedInfos[videoDocument.OwnerSharedInfoId] = sharedInfo;
                    }

                    // Create video dto.
                    videoDtos.Add(new VideoDto(
                        videoDocument,
                        sharedInfo,
                        null));
                }

                return Results.Json(
                    videoDtos,
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> UpdateCommentAsync(string commentId, string text) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get data.
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (currentUser, _) = await userService.FindUserAsync(address);

                var comment = await dbContext.Comments.FindOneAsync(commentId);
                if (comment.Author.Id != currentUser.Id)
                    throw new UnauthorizedAccessException("Only the owner of comment can update the content");

                comment.EditByAuthor(text);

                await dbContext.SaveChangesAsync();
                logger.UpdatedComment(commentId);

                return Results.Ok();
            });

        public Task<IResult> UpdateVideoAsync(string id, SwarmReference newReference) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videoManifest = await UpdateVideoHelperAsync(id, newReference);
                return Results.Json(
                    new VideoManifest2Dto(videoManifest),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        [Obsolete("Used only for API backwards compatibility")]
        public Task<IResult> UpdateVideoAsync_old(string id, SwarmReference newReference) =>
            ExceptionHandler.RunAsync(async () =>
            {
                var videoManifest = await UpdateVideoHelperAsync(id, newReference);
                return Results.Json(
                    new VideoManifestDto(videoManifest),
                    CommonConsts.IndexV03JsonSerializerOptions);
            });

        public Task<IResult> VoteVideAsync(string id, VoteValue value) =>
            ExceptionHandler.RunAsync(async () =>
            {
                // Get data.
                var address = await ethernaOidcClient.GetEtherAddressAsync();
                var (user, _) = await userService.FindUserAsync(address);
                var video = await dbContext.Videos.FindOneAsync(id);

                // Remove prev votes of user on this content.
                var prevVotes = await dbContext.Votes.QueryElementsAsync(elements =>
                    elements.Where(v => v.Owner.Id == user.Id && v.Video.Id == id)
                        .ToListAsync());
                foreach (var prevVote in prevVotes)
                    await dbContext.Votes.DeleteAsync(prevVote);

                // Create new vote.
                if (value != VoteValue.Neutral)
                {
                    var vote = new VideoVote(user, video, value);
                    await dbContext.Votes.CreateAsync(vote);
                }

                // Update counters on video.
                var totDownvotes = await dbContext.Votes.QueryElementsAsync(elements =>
                    elements.Where(v => v.Video.Id == id && v.Value == VoteValue.Down)
                        .LongCountAsync());
                var totUpvotes = await dbContext.Votes.QueryElementsAsync(elements =>
                    elements.Where(v => v.Video.Id == id && v.Value == VoteValue.Up)
                        .LongCountAsync());

                video.TotDownvotes = totDownvotes;
                video.TotUpvotes = totUpvotes;

                await dbContext.SaveChangesAsync();

                logger.VideoVoted(user.Id, id);

                return Results.Ok();
            });

        // Helpers.
        private async Task<PaginatedEnumerableDto<Comment2Dto>> GetVideoCommentsHelperAsync(string id, int page, int take)
        {
            var paginatedComments = await dbContext.Comments.QueryPaginatedElementsAsync(
                elements => elements.Where(c => c.Video.Id == id),
                c => c.CreationDateTime,
                page,
                take,
                true);

            var commentDtos = new List<Comment2Dto>();
            foreach (var comment in paginatedComments.Elements)
            {
                var author = await dbContext.Users.FindOneAsync(comment.Author.Id);
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
        
        private async Task<VideoManifest> UpdateVideoHelperAsync(string id, SwarmReference newReference)
        {
            // Get data.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var video = await dbContext.Videos.FindOneAsync(id);

            // Verify authz.
            if (video.Owner.Id != currentUser.Id)
                throw new UnauthorizedAccessException("User is not owner of the video");

            // Create videoManifest.
            var videoManifest = new VideoManifest(newReference);
            await dbContext.VideoManifests.CreateAsync(videoManifest);

            // Add manifest to video.
            video.AddManifest(videoManifest);
            await dbContext.SaveChangesAsync();

            // Create Validation Manifest Task.
            backgroundJobClient.Create<IVideoManifestValidatorTask>(
                task => task.RunAsync(video.Id, newReference.ToString()),
                new EnqueuedState(Queues.METADATA_VIDEO_VALIDATOR));

            logger.UpdatedVideo(id, newReference);

            return videoManifest;
        }
    }
}