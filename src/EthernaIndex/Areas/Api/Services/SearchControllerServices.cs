using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class SearchControllerServices : ISearchControllerServices
    {
        // Fields.
        private readonly IElasticSearchService elasticSearchService;
        private readonly IIndexDbContext indexDbContext;
        private readonly ILogger<SearchControllerServices> logger;
        private readonly ISharedDbContext sharedDbContext;

        // Constructors.
        public SearchControllerServices(
            IElasticSearchService elasticSearchService,
            IIndexDbContext indexContext,
            ILogger<SearchControllerServices> logger,
            ISharedDbContext sharedDbContext)
        {
            this.elasticSearchService = elasticSearchService;
            this.indexDbContext = indexContext;
            this.logger = logger;
            this.sharedDbContext = sharedDbContext;
        }

        // Methods.
        public async Task<IEnumerable<VideoDto>> SearchVideoAsync(string searchData, int page, int take)
        {
            var videoIds = (await elasticSearchService.SearchVideoAsync(searchData, page, take)).Select(v => v.VideoId);

            // Get videos with valid manifest.
            var videos = await indexDbContext.Videos.QueryElementsAsync(elements =>
                elements.Where(ui => videoIds.Contains(ui.Id))
                        .ToListAsync());

            // Get user info from video selected.
            var cacheSharedInfos = new Dictionary<string, UserSharedInfo>();
            var videoDtos = new List<VideoDto>();
            foreach (var video in videos)
            {
                // Get shared info.
                if (!cacheSharedInfos.ContainsKey(video.Owner.SharedInfoId))
                {
                    cacheSharedInfos[video.Owner.SharedInfoId] = await sharedDbContext.UsersInfo.FindOneAsync(video.Owner.SharedInfoId);
                }

                // Create video dto.
                videoDtos.Add(new VideoDto(
                    video,
                    video.LastValidManifest,
                    cacheSharedInfos[video.Owner.SharedInfoId],
                    null));
            }

            return videoDtos;
        }
    }
}
