using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Extensions;
using Etherna.MongODM.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class CommentMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<Comment>("0.2.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(c => c.Owner, UserMap.InformationSerializer(dbContext));
                    cm.SetMemberSerializer(v => v.Video, VideoMap.InformationSerializer(dbContext));
                });
        }
    }
}
