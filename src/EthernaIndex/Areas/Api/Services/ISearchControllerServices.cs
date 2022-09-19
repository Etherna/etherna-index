using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface ISearchControllerServices
    {
        void ReindexAllVideos();
        Task<IEnumerable<VideoDto>> SearchVideoAsync(string query, int page, int take);
    }
}
