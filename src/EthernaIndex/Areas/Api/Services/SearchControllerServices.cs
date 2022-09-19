using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.EthernaIndex.Services.Tasks;
using Hangfire;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class SearchControllerServices : ISearchControllerServices
    {
        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IElasticSearchService elasticSearchService;
        private readonly ISharedDbContext sharedDbContext;

        // Constructors.
        public SearchControllerServices(
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
