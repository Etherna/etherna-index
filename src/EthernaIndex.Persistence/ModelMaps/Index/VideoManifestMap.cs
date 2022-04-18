//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class VideoManifestMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<ManifestBase>(
                "013c3e29-764c-4bc8-941c-631d8d94adec"); //dev (pre v0.3.0), published for WAM event

            dbContext.SchemaRegistry.AddModelMapsSchema<VideoManifest>(
                "ec578080-ccd2-4d49-8a76-555b10a5dad5"); //dev (pre v0.3.0), published for WAM event

            dbContext.SchemaRegistry.AddModelMapsSchema<ErrorDetail>(
                "f555eaa8-d8e1-4f23-a402-8b9ac5930832"); //v0.3.0

            dbContext.SchemaRegistry.AddModelMapsSchema<SwarmImageRaw>(
                "91ce6fdc-b59a-46bc-9ad0-7a8608cdfa1c") //v0.3.0
                .AddFallbackModelMap(mm => //dev (pre v0.3.0), published for WAM event
                {
                    mm.AutoMap();

                    // Set members with custom name.
                    mm.GetMemberMap(i => i.Blurhash).SetElementName("BlurHash");
                });

            dbContext.SchemaRegistry.AddModelMapsSchema<VideoSource>(
                "ca9caff9-df18-4101-a362-f8f449bb2aac"); //v0.3.0
        }

        /// <summary>
        /// Basic information serializer
        /// </summary>
        public static ReferenceSerializer<VideoManifest, string> BasicInformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new(dbContext, config =>
            {
                config.UseCascadeDelete = useCascadeDelete;
                config.AddModelMapsSchema<ModelBase>("23889d7e-fb5e-4245-871f-2d63baed10d1");
                config.AddModelMapsSchema<EntityModelBase>("c6e9e564-293a-4fc4-a3fc-c4a05827f5e7", mm =>
                {
                    mm.MapMember(m => m.CreationDateTime);
                });
                config.AddModelMapsSchema<EntityModelBase<string>>("d71a8188-9baa-4454-8fba-9cc547bee291", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<ManifestBase>("fa2c6046-6b74-41bc-bba6-a3c98b501ec6", mm => 
                {
                    mm.MapMember(m => m.IsValid);
                    mm.MapMember(m => m.Manifest);
                });
                config.AddModelMapsSchema<VideoManifest>("f7966611-14aa-4f18-92f4-8697b4927fb6", mm => {
                    mm.MapMember(m => m.Title);
                });
            });

        /// <summary>
        /// Reference serializer
        /// </summary>
        public static ReferenceSerializer<VideoManifest, string> ReferenceSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new(dbContext, config =>
            {
                config.UseCascadeDelete = useCascadeDelete;
                config.AddModelMapsSchema<ModelBase>("9d763be2-d9d8-41e9-98d2-1c82b8b9d079");
                config.AddModelMapsSchema<EntityModelBase>("05025549-b611-459a-9be5-78f22f48abc6", mm => { });
                config.AddModelMapsSchema<EntityModelBase<string>>("64b18b6a-440d-47cb-9134-0a252223041e", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<ManifestBase>("2ca48f4b-8333-4ba9-b005-c97ac723511e", mm => { });
                config.AddModelMapsSchema<VideoManifest>("0e88001c-6972-46cf-9756-097a09b09011", mm => { });
            });
    }
}
