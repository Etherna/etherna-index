using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Persistence.ModelMaps.Index;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class ReviewBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<ReviewBase>("9f72b89d-ce18-417f-a2c2-bc05de28ef79",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(c => c.ReviewAuthor, UserMap.InformationSerializer(dbContext));
                });
        }
    }
}
