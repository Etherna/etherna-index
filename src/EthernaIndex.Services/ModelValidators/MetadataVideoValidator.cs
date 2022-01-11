using Etherna.EthernaIndex.Domain.Interfaces;
using Etherna.EthernaIndex.Domain.Models.Meta;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.ModelValidators
{
    public class MetadataVideoValidator : IMetadataVideoValidator
    {
        public Task<bool> CheckVideoFormatAsync(MetadataVideo metadataVideo)
        {
            return Task.FromResult(true);
        }
    }
}
