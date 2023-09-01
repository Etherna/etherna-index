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

        public async Task<PaginatedEnumerableDto<Video2Dto>> SearchVideoAsync(string query, int page, int take)
        {
            var videoDocuments = await elasticSearchService.SearchVideoAsync(query, page, take);

            // Get user info from video selected.
            var cacheSharedInfos = new Dictionary<string, UserSharedInfo>();
            var videoDtos = new List<Video2Dto>();
            foreach (var videoDocument in videoDocuments.Results)
            {
                // Get shared info.
                if (!cacheSharedInfos.ContainsKey(videoDocument.OwnerSharedInfoId))
                {
                    cacheSharedInfos[videoDocument.OwnerSharedInfoId] = await sharedDbContext.UsersInfo.FindOneAsync(videoDocument.OwnerSharedInfoId);
                }

                // Create video dto.
                videoDtos.Add(new Video2Dto(
                    videoDocument,
                    cacheSharedInfos[videoDocument.OwnerSharedInfoId],
                    null));
            }

            return new PaginatedEnumerableDto<Video2Dto>(
                page,
                videoDtos,
                take,
                videoDocuments.TotalElements);
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
                if (!cacheSharedInfos.ContainsKey(videoDocument.OwnerSharedInfoId))
                {
                    cacheSharedInfos[videoDocument.OwnerSharedInfoId] = await sharedDbContext.UsersInfo.FindOneAsync(videoDocument.OwnerSharedInfoId);
                }

                // Create video dto.
                videoDtos.Add(new VideoDto(
                    videoDocument,
                    cacheSharedInfos[videoDocument.OwnerSharedInfoId],
                    null));
            }

            return videoDtos;
        }
    }
}
