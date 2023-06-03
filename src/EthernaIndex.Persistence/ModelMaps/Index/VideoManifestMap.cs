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
using Etherna.MongODM.Core.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class VideoManifestMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<ManifestBase>(
                "1499312a-6447-437b-888e-8163d0a5b933") //v0.3.4
                .AddSecondarySchema(
                    "013c3e29-764c-4bc8-941c-631d8d94adec", //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set renamed members.
                        mm.GetMemberMap(m => m.ValidationErrors).SetElementName("ErrorValidationResults");
                    });

            dbContext.MapRegistry.AddModelMap<VideoManifest>(
                "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e") //v0.3.4
                .AddSecondarySchema(
                    "dc33442b-ae1e-428b-8b63-5dafbf192ba8", //v0.3.0
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with conversions.
                        //duration was float
                        mm.GetMemberMap(m => m.Duration).SetSerializer(
                            new NullableSerializer<long>(
                                new Int64Serializer(BsonType.Int64, new RepresentationConverter(false, true))));
                    },
                    baseSchemaId: "013c3e29-764c-4bc8-941c-631d8d94adec")
                .AddSecondarySchema(
                    "ec578080-ccd2-4d49-8a76-555b10a5dad5",  //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with conversions.
                        //duration was float
                        mm.GetMemberMap(m => m.Duration).SetSerializer(
                            new NullableSerializer<long>(
                                new Int64Serializer(BsonType.Int64, new RepresentationConverter(false, true))));
                    },
                    baseSchemaId: "013c3e29-764c-4bc8-941c-631d8d94adec");

            dbContext.MapRegistry.AddModelMap<ErrorDetail>(
                "f555eaa8-d8e1-4f23-a402-8b9ac5930832"); //v0.3.0

            dbContext.MapRegistry.AddModelMap<SwarmImageRaw>(
                "36966654-d85c-455b-b870-7b49e1124e6d") //v0.3.9
                .AddSecondarySchema("91ce6fdc-b59a-46bc-9ad0-7a8608cdfa1c", //v0.3.0
                fixDeserializedModelFunc: m =>
                {
                    if (m.ExtraElements is not null &&
                        m.ExtraElements.TryGetValue("Sources", out object? sources))
                    {
                        var castedSource = (Dictionary<string, object>)sources;
                        var sourcesV2 = new List<ImageSource>();
                        foreach (var item in castedSource)
                        {
                            sourcesV2.Add(new ImageSource(
#pragma warning disable CA1305
                                                Convert.ToInt32(item.Key.Replace("w", "", StringComparison.OrdinalIgnoreCase)),
#pragma warning restore CA1305
                                                null,
                                                null,
                                                item.Value.ToString()));
                        }
                        ReflectionHelper.SetValue(m, m => m.SourcesV2, sourcesV2);
                    }
                    
                    return Task.FromResult(m);
                })
                .AddFallbackSchema(mm => //dev (pre v0.3.0), published for WAM event
                {
                    mm.AutoMap();

                    // Set members with custom name.
                    mm.GetMemberMap(i => i.Blurhash).SetElementName("BlurHash");
                });

            dbContext.MapRegistry.AddModelMap<VideoSource>(
                "ca9caff9-df18-4101-a362-f8f449bb2aac"); //v0.3.0

            dbContext.MapRegistry.AddModelMap<ImageSource>(
                "1fbae0a8-9ee0-40f0-a8ad-21a0083fcb66"); //v0.3.9
        }

        /// <summary>
        /// Preview information serializer
        /// </summary>
        public static ReferenceSerializer<VideoManifest, string> PreviewInfoSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("23889d7e-fb5e-4245-871f-2d63baed10d1");
                config.AddModelMap<EntityModelBase>("c6e9e564-293a-4fc4-a3fc-c4a05827f5e7", mm =>
                {
                    mm.MapMember(m => m.CreationDateTime);
                });
                config.AddModelMap<EntityModelBase<string>>("d71a8188-9baa-4454-8fba-9cc547bee291", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<ManifestBase>("fa2c6046-6b74-41bc-bba6-a3c98b501ec6", mm => 
                {
                    mm.MapMember(m => m.IsValid);
                    mm.MapMember(m => m.Manifest);
                });
                config.AddModelMap<VideoManifest>("f7966611-14aa-4f18-92f4-8697b4927fb6", mm =>
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
        public static ReferenceSerializer<VideoManifest, string> ReferenceSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("753d17b7-20b9-40d2-a076-3df48b463465");
                config.AddModelMap<EntityModelBase>("7f7d2ecf-6950-4577-8a83-6d803353d762", mm => { });
                config.AddModelMap<EntityModelBase<string>>("e70b01d6-fec4-4cb6-9a1b-24406ccb058d", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<ManifestBase>("c8041883-aee4-4da7-8a69-a1fd45db2978", mm => { });
                config.AddModelMap<VideoManifest>("1ca89e6c-716c-4936-b7dc-908c057a3e41", mm => { });
            });
    }
}
