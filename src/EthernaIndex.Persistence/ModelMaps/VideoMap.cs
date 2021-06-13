﻿using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Extensions;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class VideoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<Video>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(v => v.EncryptionKey!, new HexToBinaryDataSerializer());
                    cm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContext));
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
                    cm.MapMember(v => v.FairDrivePath);
                    cm.MapMember(v => v.ManifestHash);
                })
                .RegisterProxyType<Video>();
    }
}
