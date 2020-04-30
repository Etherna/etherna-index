using Digicando.MongODM;
using Digicando.MongODM.Extensions;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Serialization.Serializers;
using Etherna.EthernaIndex.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ClassMaps
{
    class ChannelSerializers : IModelSerializerCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<Channel>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set creator.
                    cm.SetCreator(() => dbContext.ProxyGenerator.CreateInstance<Channel>(dbContext));

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(c => c.Videos,
                        new EnumerableSerializer<Video>(
                            VideoSerializers.InformationSerializer(dbContext, true)));
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<Channel, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new ReferenceSerializer<Channel, string>(dbContext, useCascadeDelete)
                .RegisterType<ModelBase>()
                .RegisterType<EntityModelBase>(cm => { })
                .RegisterType<EntityModelBase<string>>(cm =>
                {
                    cm.MapIdMember(m => m.Id);
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                })
                .RegisterType<Channel>(cm =>
                {
                    cm.MapMember(c => c.Address);
                });
    }
}
