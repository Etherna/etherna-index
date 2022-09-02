using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface ISearchControllerServices
    {
        Task<IEnumerable<VideoDto>> SearchVideoAsync(string searchData, int page, int take);
    }
}
