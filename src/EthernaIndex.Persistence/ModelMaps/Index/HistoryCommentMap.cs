using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.CommentAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class HistoryCommentMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<HistoryComment>(
                "3bf0d845-a50c-4da1-acf9-8025975e3bed", //v0.3.10
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(v => v.UserModerator!, UserMap.InformationSerializer(dbContext));
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<HistoryComment, string> InformationSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("bca9e9bf-8bde-4e33-ac96-97766b260e7f");
                config.AddModelMap<EntityModelBase>("810b0a29-4e14-415b-9fac-d6471851e75a", mm => { });
                config.AddModelMap<EntityModelBase<string>>("a4728761-ec4b-4314-acca-20d28baa8d52", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<User>("35797d00-41df-4bc8-874a-1d1d6c37d3b9", mm =>
                {
                    mm.MapMember(u => u.SharedInfoId);
                });
            });
    }
}
