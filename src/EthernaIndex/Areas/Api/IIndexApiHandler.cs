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

using Etherna.EthernaIndex.Domain.Models;
using Etherna.SwarmSdk.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api
{
    internal interface IIndexApiHandler
    {
        Task<IResult> AuthorDeleteVideoAsync(string id);
        
        Task<IResult> CreateCommentAsync(string id, string text);
        
        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> CreateCommentAsync_old(string id, string text);
        
        Task<IResult> CreateVideoAsync(SwarmReference manifestReference, PostageBatchId? batchId);
        
        Task<IResult> DeleteOwnedCommentAsync(string id);

        Task<IResult> FindUserByEthAddressAsync(EthAddress address);

        Task<IResult> FindVideoByIdAsync(string id);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> FindVideoByIdAsync_old(string id);

        Task<IResult> FindVideoByManifestReferenceAsync(SwarmReference reference);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> FindVideoByManifestReferenceAsync_old(SwarmReference reference);

        Task<IResult> ForceVideoManifestValidationAsync(string id);

        Task<IResult> ForceVideoManifestValidationAsync(SwarmReference reference);

        Task<IResult> GetCurrentUserAsync();
        
        Task<IResult> GetBulkVideoValidationStatusByIdsAsync(IEnumerable<string> ids);
        
        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetBulkVideoValidationStatusByIdsAsync_old(IEnumerable<string> ids);
        
        Task<IResult> GetBulkVideoValidationStatusByReferencesAsync(IEnumerable<SwarmReference> references);

        Task<IResult> GetIndexParameters();

        Task<IResult> GetLastUploadedVideosAsync(int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetLastUploadedVideosAsync_old(int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetLastUploadedVideosAsync_old2(int page, int take);

        Task<IResult> GetUsersAsync(int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetUsersAsync_old(int page, int take);

        Task<IResult> GetVideoCommentsAsync(string id, int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetVideoCommentsAsync_old(string id, int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetVideoCommentsAsync_old2(string id, int page, int take);

        Task<IResult> GetVideosAsync(EthAddress address, int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetVideosAsync_old(EthAddress address, int page, int take);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetVideosAsync_old2(EthAddress address, int page, int take);

        Task<IResult> GetVideoValidationStatusByIdAsync(string id);

        Task<IResult> GetVideoValidationStatusByReferenceAsync(SwarmReference reference);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetVideoValidationStatusByIdAsync_old(string id);

        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> GetVideoValidationStatusByIdAsync_old2(string id);
        
        Task<IResult> ModerateCommentAsync(string id);
        
        Task<IResult> ModerateVideoAsync(string id);

        Task<IResult> RebuildElasticIndexes();

        Task<IResult> ReindexElasticDocuments();

        Task<IResult> ReportVideoAsync(string id, SwarmReference reference, string description);

        Task<IResult> SearchVideoAsync(string query, int page, int take);
        
        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> SearchVideoAsync_old(string query, int page, int take);

        Task<IResult> UpdateCommentAsync(string commentId, string text);
        
        Task<IResult> UpdateVideoAsync(string id, SwarmReference newReference);
        
        [Obsolete("Used only for API backwards compatibility")]
        Task<IResult> UpdateVideoAsync_old(string id, SwarmReference newReference);
        
        Task<IResult> VoteVideAsync(string id, VoteValue value);
    }
}