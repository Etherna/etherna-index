using Etherna.EthernaIndex.Domain.Models.Meta;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Domain.Interfaces
{
    public interface IMetadataVideoValidator
    {
        public Task<bool> CheckVideoFormatAsync(MetadataVideo metadataVideo);
    }
}
