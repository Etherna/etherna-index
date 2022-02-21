using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class UnsuitableReportBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<UnsuitableReportBase>("d658ffcf-91ea-4e5e-b163-92eabb5490cc",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(c => c.ReporterAuthor, UserMap.InformationSerializer(dbContext));
                });
        }
    }
}
