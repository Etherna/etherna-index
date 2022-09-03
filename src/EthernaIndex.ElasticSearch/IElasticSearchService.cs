using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task IndexVideoAsync(Video video);
        Task RemoveVideoIndexAsync(Video video);
        Task RemoveVideoIndexAsync(string videoId);
        Task<IEnumerable<VideoDocument>> SearchVideoAsync(string searchData, int page, int take);
    }
}
