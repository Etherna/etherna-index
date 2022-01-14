using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Interfaces
{
    public interface IMetadataVideoValidatorTask
    {
        Task RunAsync(string manifestHash);
    }
}
