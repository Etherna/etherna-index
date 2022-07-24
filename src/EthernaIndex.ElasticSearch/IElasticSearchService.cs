using Etherna.EthernaIndex.ElasticSearch.DtoModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public interface IElasticSearchService
    {
        public Task<IEnumerable<VideoManifestElasticDto>> FindVideoAsync(FindVideoDto findVideoDto);
        public Task IndexAsync(VideoManifestElasticDto document);
        public Task RemoveVideoIndexAsync(string videoId);
    }
}
