using Digicando.MongODM;
using Digicando.MongODM.ProxyModels;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Utility;
using Etherna.EthernaIndex.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ClassMaps
{
    class ModelBaseSerializers : IModelSerializerCollector
    {
        public void Register(IDBCache dbCache,
            IDbContext dbContext,
            IDocumentSchemaRegister documentSchemaRegister,
            IProxyGenerator proxyGenerator)
        {
            // register class maps.
            documentSchemaRegister.RegisterModelSchema<ModelBase>("0.1.0");

            documentSchemaRegister.RegisterModelSchema<EntityModelBase<string>>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set Id representation.
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                  .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
