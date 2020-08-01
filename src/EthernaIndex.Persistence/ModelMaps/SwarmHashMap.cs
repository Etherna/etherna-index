using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongODM;
using Etherna.MongODM.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class SwarmHashMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<SwarmHashBase>("0.2.0");
            dbContext.DocumentSchemaRegister.RegisterModelSchema<SwarmContentHash>("0.2.0");
            dbContext.DocumentSchemaRegister.RegisterModelSchema<SwarmSocHash>("0.2.0");
        }
    }
}
