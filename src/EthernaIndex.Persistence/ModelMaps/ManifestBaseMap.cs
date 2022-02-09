using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class ManifestBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<ManifestBase>("013c3e29-764c-4bc8-941c-631d8d94adec");
        }
    }
}
