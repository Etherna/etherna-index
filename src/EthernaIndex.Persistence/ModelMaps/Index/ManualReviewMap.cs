using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class ManualReviewMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<ManualReviewBase>(
                "9f72b89d-ce18-417f-a2c2-bc05de28ef79", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(r => r.Author, UserMap.InformationSerializer(dbContext));
                });

            dbContext.SchemaRegistry.AddModelMapsSchema<ManualVideoReview>(
                "e3e734ab-d845-4ec2-8920-68956eba950d", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(r => r.Video, VideoMap.ReferenceSerializer(dbContext));
                });
        }
    }
}
