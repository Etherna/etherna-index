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
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.EthernaIndex.Services.Tasks;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal sealed class SearchControllerService : ISearchControllerService
    {
        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IElasticSearchService elasticSearchService;
        private readonly ISharedDbContext sharedDbContext;

        // Constructors.
        public SearchControllerService(
            IBackgroundJobClient backgroundJobClient,
            IElasticSearchService elasticSearchService,
            ISharedDbContext sharedDbContext)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.elasticSearchService = elasticSearchService;
            this.sharedDbContext = sharedDbContext;
        }

        // Methods.
        public void ReindexAllVideos() =>
            backgroundJobClient.Enqueue<IFullVideoReindexTask>(t => t.RunAsync());

        public async Task<PaginatedEnumerableDto<VideoPreviewDto>> SearchVideoAsync(string query, int page, int take)
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

            return new PaginatedEnumerableDto<VideoPreviewDto>(
                page,
                videoDtos,
                take,
                paginatedVideoDocuments.TotalElements);
        }

        //deprecated
        [Obsolete("Used only for API backwards compatibility")]
        public async Task<IEnumerable<VideoDto>> SearchVideoAsync_old(string query, int page, int take)
        {
            var videoDocuments = await elasticSearchService.SearchVideoAsync(query, page, take);

            // Get user info from video selected.
            var cacheSharedInfos = new Dictionary<string, UserSharedInfo>();
            var videoDtos = new List<VideoDto>();
            foreach (var videoDocument in videoDocuments.Results)
            {
                // Get shared info.
                if (!cacheSharedInfos.TryGetValue(videoDocument.OwnerSharedInfoId, out UserSharedInfo? sharedInfo))
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

            return videoDtos;
        }
    }
}
