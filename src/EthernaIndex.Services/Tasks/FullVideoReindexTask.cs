using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    internal class FullVideoReindexTask : IFullVideoReindexTask
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IElasticSearchService elasticSearchService;

        // Constructor.
        public FullVideoReindexTask(
            IIndexDbContext dbContext,
            IElasticSearchService elasticSearchService)
        {
            this.dbContext = dbContext;
            this.elasticSearchService = elasticSearchService;
        }

        // Methods.
        public async Task RunAsync()
        {
            var videosCursor = await dbContext.Videos.FindAsync<Video>(Builders<Video>.Filter.Empty, new() { NoCursorTimeout = true });
            while (await videosCursor.MoveNextAsync())
                foreach (var element in videosCursor.Current.Where(v => v.LastValidManifest != null))
                    await elasticSearchService.IndexVideoAsync(element);
        }
    }
}
