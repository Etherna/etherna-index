using Etherna.EthernaIndex.ElasticSearch.DtoModel;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<VideoManifestElasticDto>> FindVideoAsync(FindVideoDto findVideoDto)
        {
            if (findVideoDto is null)
                throw new ArgumentNullException(nameof(findVideoDto));

            if (findVideoDto.Page == 0)
                findVideoDto.Page = 1;

            if (string.IsNullOrWhiteSpace(findVideoDto.Title) &&
                string.IsNullOrWhiteSpace(findVideoDto.Description))
            {
                throw new InvalidOperationException($"Title or description is required for find");
            }

            var searchResponse = await elasticClient.SearchAsync<VideoManifestElasticDto>(s =>
                s.Query(q => q.Bool(b => 
                    Filter(b,
                    findVideoDto))
                )
                .From((findVideoDto.Page - 1) * findVideoDto.Take)
                .Size(findVideoDto.Take));

            return searchResponse.Documents;
        }

        private static BoolQueryDescriptor<VideoManifestElasticDto> Filter(
            BoolQueryDescriptor<VideoManifestElasticDto> b,
            FindVideoDto dtoSearch)
        {
            if (dtoSearch.FilterType == FilterType.FilterInOr)
                return b.Must(mu => QueryOr(mu, dtoSearch));
            else
                return b.Must(mu => QueryAnd(mu, dtoSearch));
        }

        public async Task IndexAsync(VideoManifestElasticDto document)
        {
            if (document is null)
                throw new ArgumentException(nameof(VideoManifestElasticDto));

            var documentIndexed = await elasticClient.GetAsync<VideoManifestElasticDto>(document.VideoId);
            if (documentIndexed?.Source is not null &&
                documentIndexed.Source.CreationDateTime > document.CreationDateTime)
            {
                return;
            }

            await elasticClient.IndexDocumentAsync(document);
        }

        private static QueryContainer QueryOr(
            QueryContainerDescriptor<VideoManifestElasticDto> mu,
            FindVideoDto dtoSearch)
        {
            QueryContainer? result = null;
#pragma warning disable CA1308 // Wildcard require strings to lower
            if (!string.IsNullOrWhiteSpace(dtoSearch.Title))
            {
                result = mu.Wildcard(f => f.Title, '*' + dtoSearch.Title.ToLowerInvariant() + '*');
            }
            if (!string.IsNullOrWhiteSpace(dtoSearch.Description))
            {
                result = result == null ? 
                        mu.Wildcard(f => f.Description, '*' + dtoSearch.Description.ToLowerInvariant() + '*') :
                        result || mu.Wildcard(f => f.Description, '*' + dtoSearch.Description.ToLowerInvariant() + '*');
            }
#pragma warning restore CA1308 // 
            return result!;
        }

        private static QueryContainer QueryAnd(
            QueryContainerDescriptor<VideoManifestElasticDto> mu, 
            FindVideoDto dtoSearch)
        {
            QueryContainer? result = null;
#pragma warning disable CA1308 // Wildcard require strings to lower
            if (!string.IsNullOrWhiteSpace(dtoSearch.Title))
            {
                result = mu.Wildcard(f => f.Title, '*' + dtoSearch.Title.ToLowerInvariant() + '*');
            }
            if (!string.IsNullOrWhiteSpace(dtoSearch.Description))
            {
                result = result == null ?
                        mu.Wildcard(f => f.Description, '*' + dtoSearch.Description.ToLowerInvariant() + '*') :
                        result && mu.Wildcard(f => f.Description, '*' + dtoSearch.Description.ToLowerInvariant() + '*');
            }
#pragma warning restore CA1308 // 
            return result!;
        }

        public async Task RemoveVideoIndexAsync(string videoId)
        {
            await elasticClient.DeleteAsync<VideoManifestElasticDto>(videoId);
        }
    }
}
