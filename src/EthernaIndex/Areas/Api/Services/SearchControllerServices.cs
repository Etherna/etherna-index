using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.ElasticSearch;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class SearchControllerServices : ISearchControllerServices
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;
        private readonly ILogger<SearchControllerServices> logger;
        private readonly ISharedDbContext sharedDbContext;

        // Constructors.
        public SearchControllerServices(
            IElasticSearchService elasticSearchService,
            ILogger<SearchControllerServices> logger,
            ISharedDbContext sharedDbContext)
        {
            this.elasticSearchService = elasticSearchService;
            this.logger = logger;
            this.sharedDbContext = sharedDbContext;
        }

        // Methods.
        public async Task<IEnumerable<VideoDto>> SearchVideoAsync(string query, int page, int take)
        {
            var videoDocuments = await elasticSearchService.SearchVideoAsync(query, page, take);

            // Get user info from video selected.
            var cacheSharedInfos = new Dictionary<string, UserSharedInfo>();
            var videoDtos = new List<VideoDto>();
            foreach (var videoDocument in videoDocuments)
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
