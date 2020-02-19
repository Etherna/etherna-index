using Digicando.MongODM;
using Digicando.MongODM.Extensions;
using Digicando.MongODM.ProxyModels;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Serialization.Serializers;
using Digicando.MongODM.Utility;
using Etherna.EthernaIndex.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ClassMaps
{
    class VideoSerializers : IModelSerializerCollector
    {
        public void Register(
            IDBCache dbCache,
            IDbContext dbContext,
            IDocumentSchemaRegister documentSchemaRegister,
            IProxyGenerator proxyGenerator)
        {
            documentSchemaRegister.RegisterModelSchema<Video>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set creator.
                    cm.SetCreator(() => proxyGenerator.CreateInstance<Video>(dbContext));

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(v => v.OwnerChannel, ChannelSerializers.InformationSerializer(dbContext));
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<Video, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new ReferenceSerializer<Video, string>(dbContext, useCascadeDelete)
                .RegisterType<ModelBase>()
                .RegisterType<EntityModelBase>(cm => { })
                .RegisterType<EntityModelBase<string>>(cm =>
                {
                    cm.MapIdMember(m => m.Id);
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                })
                .RegisterType<Video>(cm =>
                {
                    cm.MapMember(v => v.ThumbnailHash);
                    cm.MapMember(v => v.Title);
                    cm.MapMember(v => v.VideoHash);
                });
    }
}
