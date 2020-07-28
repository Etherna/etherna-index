﻿using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Extensions;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using System.Threading.Tasks;

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
                    cm.SetMemberSerializer(v => v.OwnerChannel, ChannelMap.InformationSerializer(dbContext));

                    // Set member to ignore if default.
                    cm.GetMemberMap(v => v.ThumbnailHashIsRaw).SetIgnoreIfDefault(true);
                    cm.GetMemberMap(v => v.VideoHashIsRaw).SetIgnoreIfDefault(true);
                },
                modelMigrationAsync: (video, semver) =>
                {
                    // 0.2.0 fixes.
                    if (semver < "0.2.0")
                    {
                        ReflectionHelper.SetValue(
                            video, v => v.VideoHashIsRaw, true);
                        ReflectionHelper.SetValue(
                            video, v => v.ThumbnailHashIsRaw, true);
                    }

                    return Task.FromResult(video);
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
