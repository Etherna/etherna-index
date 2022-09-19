using Hangfire;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public interface IFullVideoReindexTask
    {
        [Queue(Queues.ELASTIC_SEARCH_MAINTENANCE)]
        Task RunAsync();
    }
}