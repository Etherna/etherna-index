using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task IndexCommentAsync(Comment comment);
        Task IndexVideoAsync(Video video);
        Task RemoveCommentIndexAsync(Comment comment);
        Task RemoveCommentIndexAsync(string commentId);
        Task RemoveVideoIndexAsync(Video video);
        Task RemoveVideoIndexAsync(string videoId);
        Task<IEnumerable<VideoDocument>> SearchVideoAsync(string query, int page, int take);
    }
}
