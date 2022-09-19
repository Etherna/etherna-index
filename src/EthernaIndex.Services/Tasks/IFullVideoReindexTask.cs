using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public interface IFullVideoReindexTask
    {
        Task RunAsync();
    }
}