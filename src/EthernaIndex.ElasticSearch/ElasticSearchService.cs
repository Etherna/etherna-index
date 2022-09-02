using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Nest;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        // Fields.
        private readonly IElasticClient elasticClient;

        // Constructors.
        public ElasticSearchService(IElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        // Public methods.
        public async Task IndexVideoAsync(Video video)
        {
            if (video.LastValidManifest is null)
                throw new NullReferenceException(nameof(video.LastValidManifest));

            var document = new VideoManifestDocument(video, video.LastValidManifest);

            await elasticClient.IndexDocumentAsync(document);
        }

        public async Task RemoveVideoIndexAsync(Video video)
        {
            await RemoveVideoIndexAsync(video.Id);
        }

        public async Task RemoveVideoIndexAsync(string videoId)
        {
            await elasticClient.DeleteAsync<VideoManifestDocument>(videoId);
        }

        public async Task<IEnumerable<VideoManifestDocument>> SearchVideoAsync(string searchData, int page, int take)
        {
            if (string.IsNullOrWhiteSpace(searchData))
                throw new ArgumentNullException(searchData);
            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (take <= 0)
                throw new ArgumentOutOfRangeException(nameof(take));

            var searchResponse = await elasticClient.SearchAsync<VideoManifestDocument>(s =>
                s.Query(q => q.Bool(b =>
                    Filter(b, searchData))
                )
                .From(page * take)
                .Size(take));

            return searchResponse.Documents;
        }

        // Private methods.
        private static BoolQueryDescriptor<VideoManifestDocument> Filter(
            BoolQueryDescriptor<VideoManifestDocument> b,
            string searchData)
        {
            return b.Must(mu => 
                mu.Wildcard(f => f.Title, '*' + searchData.ToLowerInvariant() + '*') ||
                mu.Wildcard(f => f.Description, '*' + searchData.ToLowerInvariant() + '*')
            );
        }

    }
}
