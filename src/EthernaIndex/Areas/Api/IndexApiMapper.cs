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

using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Areas.Api.InputModels;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Extensions;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.Areas.Api
{
    public static class IndexApiMapper
    {
        // Methods.
        public static void MapIndexApi(this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            // APIs.
            ConfigureV03Maps(app.MapGroup("/api/v0.3").WithMetadata(new IndexApiMarker()));
        }

        // Helpers.
        private static void ConfigureV03Maps(RouteGroupBuilder builder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            //comments
            builder.MapDelete("comments/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.DeleteOwnedCommentAsync(id))
                .RequireAuthorization(CommonConsts.UserInteractApiScopePolicy)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
            
            //moderation
            builder.MapDelete("moderation/comments/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.ModerateCommentAsync(id))
                .RequireAuthorization(CommonConsts.RequireAdministratorRolePolicy)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapDelete("moderation/videos/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.ModerateVideoAsync(id))
                .RequireAuthorization(CommonConsts.RequireAdministratorRolePolicy)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);
            
            //search
            builder.MapGet("search/query",
                    (IIndexApiHandler handler,
                            [FromQuery] string query,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.SearchVideoAsync_old(query, page, take))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .IsDeprecated("Use \"query2\" instead");
            
            builder.MapGet("search/query2",
                    (IIndexApiHandler handler,
                            [FromQuery] string query,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.SearchVideoAsync(query, page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<VideoPreviewDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
            
            builder.MapPost("search/rebuild",
                    (IIndexApiHandler handler) =>
                        handler.RebuildElasticIndexes())
                .RequireAuthorization(CommonConsts.RequireAdministratorRolePolicy)
                .WithSummary("Rebuild Elasticsearch indexes from scratch")
                .WithDescription(
                    "Enqueues a background job that drops the Elasticsearch indexes, recreates them and " +
                    "reindexes every document. Use it to apply index mapping/settings changes or to recover " +
                    "from a corrupted index. The indexes are unavailable while the job runs (downtime), so it " +
                    "is a maintenance-window operation. For routine, zero-downtime orphan reconciliation use " +
                    "\"search/reindex\" instead.")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            builder.MapPost("search/reindex",
                    (IIndexApiHandler handler) =>
                        handler.ReindexElasticDocuments())
                .RequireAuthorization(CommonConsts.RequireAdministratorRolePolicy)
                .WithSummary("Reindex Elasticsearch documents and prune orphans")
                .WithDescription(
                    "Enqueues a background job that reindexes every document in place and then removes orphan " +
                    "documents (still indexed but no longer present in the database). Search stays available for " +
                    "the whole duration (zero-downtime). It does not apply index structure changes: for " +
                    "mapping/settings migrations or to rebuild a corrupted index use \"search/rebuild\".")
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized);

            //system
            builder.MapGet("system/parameters",
                    (IIndexApiHandler handler) =>
                        handler.GetIndexParameters())
                .AllowAnonymous()
                .Produces<SystemParametersDto>();
            
            builder.MapPut("system/validate/manifest/{reference}",
                    (IIndexApiHandler handler,
                            [FromRoute] SwarmReference reference) =>
                        handler.ForceVideoManifestValidationAsync(reference))
                .RequireAuthorization(CommonConsts.RequireAdministratorRolePolicy)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapPut("system/validate/video/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.ForceVideoManifestValidationAsync(id))
                .RequireAuthorization(CommonConsts.RequireAdministratorRolePolicy)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            //users
            builder.MapGet("users",
                    (IIndexApiHandler handler,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetUsersAsync_old(page, take))
                .AllowAnonymous()
                .Produces<IEnumerable<UserDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .IsDeprecated("Use \"list2\" instead");
            
            builder.MapGet("users/list2",
                    (IIndexApiHandler handler,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetUsersAsync(page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<UserDto>>()
                .Produces(StatusCodes.Status400BadRequest);
            
            builder.MapGet("users/{address}",
                    (IIndexApiHandler handler,
                            [FromRoute] EthAddress address) =>
                        handler.FindUserByEthAddressAsync(address))
                .AllowAnonymous()
                .Produces<UserDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapGet("users/{address}/videos",
                    (IIndexApiHandler handler,
                            [FromRoute] EthAddress address,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetVideosAsync_old(address, page, take))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"videos3\" instead");
            
            builder.MapGet("users/{address}/videos2",
                    (IIndexApiHandler handler,
                            [FromRoute] EthAddress address,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetVideosAsync_old2(address, page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<VideoDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"videos3\" instead");
            
            builder.MapGet("users/{address}/videos3",
                    (IIndexApiHandler handler,
                            [FromRoute] EthAddress address,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetVideosAsync(address, page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<Video2Dto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapGet("users/current",
                    (IIndexApiHandler handler) =>
                        handler.GetCurrentUserAsync())
                .Produces<CurrentUserDto>();

            //videos
            builder.MapGet("videos/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.FindVideoByIdAsync_old(id))
                .AllowAnonymous()
                .Produces<VideoDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"find2\" instead");
            
            builder.MapGet("videos/{id}/find2",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.FindVideoByIdAsync(id))
                .AllowAnonymous()
                .Produces<Video2Dto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapGet("videos/{id}/comments",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetVideoCommentsAsync_old(id, page, take))
                .AllowAnonymous()
                .Produces<IEnumerable<CommentDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"{id}/comments3\" instead");
            
            builder.MapGet("videos/{id}/comments2",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetVideoCommentsAsync_old2(id, page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<CommentDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"{id}/comments3\" instead");
            
            builder.MapGet("videos/{id}/comments3",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetVideoCommentsAsync(id, page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<Comment2Dto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapGet("videos/{id}/validations",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.GetVideoValidationStatusByIdAsync_old(id))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoManifestStatusDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"{id}/validation2\" instead");
            
            builder.MapGet("videos/{id}/validation",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.GetVideoValidationStatusByIdAsync_old2(id))
                .AllowAnonymous()
                .Produces<VideoStatusDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"{id}/validation2\" instead");
            
            builder.MapGet("videos/{id}/validation2",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.GetVideoValidationStatusByIdAsync(id))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoManifestStatusDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapGet("videos/latest",
                    (IIndexApiHandler handler,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetLastUploadedVideosAsync_old(page, take))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .IsDeprecated("Use \"latest3\" instead");
            
            builder.MapGet("videos/latest2",
                    (IIndexApiHandler handler,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetLastUploadedVideosAsync_old2(page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<VideoDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .IsDeprecated("Use \"latest3\" instead");
            
            builder.MapGet("videos/latest3",
                    (IIndexApiHandler handler,
                            [FromQuery, Range(0, int.MaxValue)] int page = 0,
                            [FromQuery, Range(1, 100)] int take = 25) =>
                        handler.GetLastUploadedVideosAsync(page, take))
                .AllowAnonymous()
                .Produces<PaginatedEnumerableDto<VideoPreviewDto>>()
                .Produces(StatusCodes.Status400BadRequest);
            
            builder.MapGet("videos/manifest/{reference}",
                    (IIndexApiHandler handler,
                            [FromRoute] SwarmReference reference) =>
                        handler.FindVideoByManifestReferenceAsync_old(reference))
                .AllowAnonymous()
                .Produces<VideoDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"manifest2\" instead");
            
            builder.MapGet("videos/manifest2/{reference}",
                    (IIndexApiHandler handler,
                            [FromRoute] SwarmReference reference) =>
                        handler.FindVideoByManifestReferenceAsync(reference))
                .AllowAnonymous()
                .Produces<Video2Dto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapGet("videos/manifest/{reference}/validation",
                    (IIndexApiHandler handler,
                            [FromRoute] SwarmReference reference) =>
                        handler.GetVideoValidationStatusByReferenceAsync(reference))
                .AllowAnonymous()
                .Produces<VideoManifestStatusDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapPost("videos",
                    (IIndexApiHandler handler,
                            [FromBody] VideoCreateInput videoInput) =>
                        handler.CreateVideoAsync(videoInput.ManifestHash, null))
                .Produces<string>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .IsDeprecated("Use \"videos/create2\" instead");
            
            builder.MapPost("videos/create2",
                    (IIndexApiHandler handler,
                            [FromBody] VideoCreateInput2 videoInput) =>
                        handler.CreateVideoAsync(videoInput.ManifestReference, videoInput.BatchId))
                .Produces<string>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
            
            builder.MapPost("videos/{id}/comments",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromBody] string text) =>
                        handler.CreateCommentAsync_old(id, text))
                .Produces<CommentDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .IsDeprecated("Use \"{id}/comments2\" instead");
            
            builder.MapPost("videos/{id}/comments2",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromBody] string text) =>
                        handler.CreateCommentAsync(id, text))
                .Produces<Comment2Dto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
            
            builder.MapPost("videos/{id}/manifest/{reference}/reports",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromRoute] SwarmReference reference,
                            [FromQuery] string description) =>
                        handler.ReportVideoAsync(id, reference, description))
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
            
            builder.MapPost("videos/{id}/votes",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromQuery] VoteValue value) =>
                        handler.VoteVideAsync(id, value))
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);
            
            builder.MapPut("videos/bulkValidation",
                    (IIndexApiHandler handler,
                            [FromBody] IEnumerable<string> ids) =>
                        handler.GetBulkVideoValidationStatusByIdsAsync_old(ids))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoStatusDto>>()
                .Produces(StatusCodes.Status400BadRequest)
                .IsDeprecated("Use \"bulkValidation2\" instead");
            
            builder.MapPut("videos/bulkValidation2",
                    (IIndexApiHandler handler,
                            [FromBody] IEnumerable<string> ids) =>
                        handler.GetBulkVideoValidationStatusByIdsAsync(ids))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoManifestStatusDto>>()
                .Produces(StatusCodes.Status400BadRequest);
            
            builder.MapPut("videos/manifest/bulkValidation",
                    (IIndexApiHandler handler,
                            [FromBody] IEnumerable<SwarmReference> references) =>
                        handler.GetBulkVideoValidationStatusByReferencesAsync(references))
                .AllowAnonymous()
                .Produces<IEnumerable<VideoManifestStatusDto>>()
                .Produces(StatusCodes.Status400BadRequest);
            
            builder.MapPut("videos/comments/{commentId}",
                    (IIndexApiHandler handler,
                            [FromRoute] string commentId,
                            [FromBody] string text) =>
                        handler.UpdateCommentAsync(commentId, text))
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapPut("videos/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromQuery(Name = "newHash")] SwarmReference newReference) =>
                        handler.UpdateVideoAsync_old(id, newReference))
                .Produces<VideoManifestDto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .IsDeprecated("Use \"update2\" instead");
            
            builder.MapPut("videos/{id}/update2",
                    (IIndexApiHandler handler,
                            [FromRoute] string id,
                            [FromQuery(Name = "newHash")] SwarmReference newReference) =>
                        handler.UpdateVideoAsync(id, newReference))
                .Produces<VideoManifest2Dto>()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);
            
            builder.MapDelete("videos/{id}",
                    (IIndexApiHandler handler,
                            [FromRoute] string id) =>
                        handler.AuthorDeleteVideoAsync(id))
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}