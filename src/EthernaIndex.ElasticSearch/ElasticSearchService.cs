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

            var document = new VideoDocument(video);

            await elasticClient.IndexDocumentAsync(document);
        }

        public async Task RemoveVideoIndexAsync(Video video) =>
            await RemoveVideoIndexAsync(video.Id);

        public async Task RemoveVideoIndexAsync(string videoId) =>
            await elasticClient.DeleteAsync<VideoDocument>(videoId);

        public async Task<IEnumerable<VideoDocument>> SearchVideoAsync(string query, int page, int take)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(query);
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (take <= 0)
                throw new ArgumentOutOfRangeException(nameof(take));

            var searchResponse = await elasticClient.SearchAsync<VideoDocument>(s =>
                s.Query(q => q.Bool(b =>
                    b.Must(mu =>
                    mu.Wildcard(f => f.Title, '*' + query.ToLowerInvariant() + '*') ||
                    mu.Wildcard(f => f.Description, '*' + query.ToLowerInvariant() + '*'))
                ))
                .From(page * take)
                .Size(take));

            return searchResponse.Documents;
        }

    }
}
