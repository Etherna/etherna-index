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
using Etherna.MongoDB.Bson.Serialization.Options;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Mapping;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class VideoManifestMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<ManifestBase>(
                "1499312a-6447-437b-888e-8163d0a5b933") //v0.3.4
                .AddSecondaryMap(new ModelMap<ManifestBase>(
                    "013c3e29-764c-4bc8-941c-631d8d94adec", //dev (pre v0.3.0), published for WAM event
                    new MongoDB.Bson.Serialization.BsonClassMap<ManifestBase>(mm =>
                    {
                        mm.AutoMap();

                        // Set renamed members.
                        mm.GetMemberMap(m => m.ValidationErrors).SetElementName("ErrorValidationResults");
                    })));

            dbContext.SchemaRegistry.AddModelMapsSchema<VideoManifest>(
                "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e") //v0.3.4
                .AddSecondaryMap(new ModelMap<VideoManifest>(
                    "dc33442b-ae1e-428b-8b63-5dafbf192ba8", //v0.3.0
                    new MongoDB.Bson.Serialization.BsonClassMap<VideoManifest>(mm =>
                    {
                        mm.AutoMap();

                        // Set members with conversions.
                        //duration was float
                        mm.GetMemberMap(m => m.Duration).SetSerializer(
                            new NullableSerializer<long>(
                                new Int64Serializer(BsonType.Int64, new RepresentationConverter(false, true))));
                    }),
                    baseModelMapId: "013c3e29-764c-4bc8-941c-631d8d94adec"))
                .AddSecondaryMap(new ModelMap<VideoManifest>(
                    "ec578080-ccd2-4d49-8a76-555b10a5dad5",  //dev (pre v0.3.0), published for WAM event
                    new MongoDB.Bson.Serialization.BsonClassMap<VideoManifest>(mm =>
                    {
                        mm.AutoMap();

                        // Set members with conversions.
                        //duration was float
                        mm.GetMemberMap(m => m.Duration).SetSerializer(
                            new NullableSerializer<long>(
                                new Int64Serializer(BsonType.Int64, new RepresentationConverter(false, true))));
                    }),
                    baseModelMapId: "013c3e29-764c-4bc8-941c-631d8d94adec"));

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
        /// Preview information serializer
        /// </summary>
        public static ReferenceSerializer<VideoManifest, string> PreviewInfoSerializer(
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
                config.AddModelMapsSchema<VideoManifest>("f7966611-14aa-4f18-92f4-8697b4927fb6", mm =>
                {
                    mm.MapMember(m => m.Duration).SetSerializer( //could be float in old documents
                        new NullableSerializer<long>(
                            new Int64Serializer(BsonType.Int64, new RepresentationConverter(false, true))));
                    mm.MapMember(m => m.Thumbnail);
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
                config.AddModelMapsSchema<ModelBase>("753d17b7-20b9-40d2-a076-3df48b463465");
                config.AddModelMapsSchema<EntityModelBase>("7f7d2ecf-6950-4577-8a83-6d803353d762", mm => { });
                config.AddModelMapsSchema<EntityModelBase<string>>("e70b01d6-fec4-4cb6-9a1b-24406ccb058d", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<ManifestBase>("c8041883-aee4-4da7-8a69-a1fd45db2978", mm => { });
                config.AddModelMapsSchema<VideoManifest>("1ca89e6c-716c-4936-b7dc-908c057a3e41", mm => { });
            });
    }
}
