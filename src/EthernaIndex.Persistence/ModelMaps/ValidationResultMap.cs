using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class ValidationResultMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<ValidationResult>("013c3e29-764c-4bc8-941c-631d8d94adec",
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContext));
                });

        }
    }
}
