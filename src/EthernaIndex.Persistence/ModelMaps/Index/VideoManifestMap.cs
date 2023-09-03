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

using Etherna.EthernaIndex.Domain.Exceptions;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class VideoManifestMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<VideoManifest>(
                "c32a815b-4667-4534-8276-eb3c1d812d09") //0.3.9
                .AddSecondarySchema(
                    "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e", //v0.3.4
                    fixDeserializedModelFunc: m =>
                    {
                    if (m.ExtraElements is null)
                        return Task.FromResult(m);

                        // Verify if there isn't any validation error.
                        if (!m.ValidationErrors.Any())
                        {
                            // Extract metadata.
                            var title = m.ExtraElements.TryGetValue("Title", out var titleObj) ?
                                (string)titleObj : "";
                            var description = m.ExtraElements.TryGetValue("Description", out var descriptionObj) ?
                                (string)descriptionObj : "";
                            var duration = (long)(m.ExtraElements["Duration"] ?? 0L);
                            var sources = m.ExtraElements.TryGetValue("Sources", out var sourcesObj) ?
                                new ExtraElementsSerializer(dbContext).DeserializeValue<List<VideoSourceV1>>(sourcesObj) :
                                new List<VideoSourceV1>();
                            var thumbnail = m.ExtraElements.TryGetValue("Thumbnail", out var thumbnailObj) ?
                                new ExtraElementsSerializer(dbContext).DeserializeValue<ThumbnailV1>(thumbnailObj) :
                                null;
                            var batchId = m.ExtraElements.TryGetValue("BatchId", out var batchIdObj) ?
                                (string?)batchIdObj : null;
                            var personalData = m.ExtraElements.TryGetValue("PersonalData", out var personalDataObj) ?
                                (string?)personalDataObj : null;

                            // Update model.
                            try
                            {
                                var metadata = new VideoManifestMetadataV1(
                                    title,
                                    description,
                                    duration,
                                    sources,
                                    thumbnail,
                                    batchId,
                                    null,
                                    null,
                                    personalData);
                                ReflectionHelper.SetValue(m, vm => vm.Metadata!, metadata);
                            }
                            catch (VideoManifestValidationException e)
                            {
                                m.FailedValidation(e.ValidationErrors);
                            }
                        }

                        return Task.FromResult(m);
                    })
                .AddSecondarySchema(
                    "dc33442b-ae1e-428b-8b63-5dafbf192ba8", //v0.3.0
                    mm =>
                    {
                        mm.AutoMap();

                        // Set renamed members.
                        mm.GetMemberMap(m => m.ValidationErrors).SetElementName("ErrorValidationResults");
                    },
                    fixDeserializedModelFunc: m =>
                    {
                        if (m.ExtraElements is null)
                            return Task.FromResult(m);

                        // Verify if there isn't any validation error.
                        if (!m.ValidationErrors.Any())
                        {
                            // Extract metadata.
                            var title = m.ExtraElements.TryGetValue("Title", out var titleObj) ?
                                (string)titleObj : "";
                            var description = m.ExtraElements.TryGetValue("Description", out var descriptionObj) ?
                                (string)descriptionObj : "";
                            var duration = (long)(double)(m.ExtraElements["Duration"] ?? 0.0); //was double
                            var sources = m.ExtraElements.TryGetValue("Sources", out var sourcesObj) ?
                                new ExtraElementsSerializer(dbContext).DeserializeValue<List<VideoSourceV1>>(sourcesObj) :
                                new List<VideoSourceV1>();
                            var thumbnail = m.ExtraElements.TryGetValue("Thumbnail", out var thumbnailObj) ?
                                new ExtraElementsSerializer(dbContext).DeserializeValue<ThumbnailV1>(thumbnailObj) :
                                null;
                            var batchId = m.ExtraElements.TryGetValue("BatchId", out var batchIdObj) ?
                                (string?)batchIdObj : null;
                            var personalData = m.ExtraElements.TryGetValue("PersonalData", out var personalDataObj) ?
                                (string?)personalDataObj : null;

                            // Update model.
                            try
                            {
                                var metadata = new VideoManifestMetadataV1(
                                    title,
                                    description,
                                    duration,
                                    sources,
                                    thumbnail,
                                    batchId,
                                    null,
                                    null,
                                    personalData);
                                ReflectionHelper.SetValue(m, vm => vm.Metadata!, metadata);
                            }
                            catch (VideoManifestValidationException e)
                            {
                                m.FailedValidation(e.ValidationErrors);
                            }
                        }

                        return Task.FromResult(m);
                    })
                .AddSecondarySchema(
                    "ec578080-ccd2-4d49-8a76-555b10a5dad5",  //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set renamed members.
                        mm.GetMemberMap(m => m.ValidationErrors).SetElementName("ErrorValidationResults");
                    },
                    fixDeserializedModelFunc: m =>
                    {
                        if (m.ExtraElements is null)
                            return Task.FromResult(m);

                        // Verify if there isn't any validation error.
                        if (!m.ValidationErrors.Any())
                        {
                            // Extract metadata.
                            var title = m.ExtraElements.TryGetValue("Title", out var titleObj) ?
                                (string)titleObj : "";
                            var description = m.ExtraElements.TryGetValue("Description", out var descriptionObj) ?
                                (string)descriptionObj : "";
                            var duration = (long)(double)(m.ExtraElements["Duration"] ?? 0.0); //was double
                            var sources = m.ExtraElements.TryGetValue("Sources", out var sourcesObj) ?
                                new ExtraElementsSerializer(dbContext).DeserializeValue<List<VideoSourceV1>>(sourcesObj) :
                                new List<VideoSourceV1>();
                            var thumbnail = m.ExtraElements.TryGetValue("Thumbnail", out var thumbnailObj) ?
                                new ExtraElementsSerializer(dbContext).DeserializeValue<ThumbnailV1>(thumbnailObj) :
                                null;
                            var batchId = m.ExtraElements.TryGetValue("BatchId", out var batchIdObj) ?
                                (string?)batchIdObj : null;
                            var personalData = m.ExtraElements.TryGetValue("PersonalData", out var personalDataObj) ?
                                (string?)personalDataObj : null;

                            // Update model.
                            try
                            {
                                var metadata = new VideoManifestMetadataV1(
                                    title,
                                    description,
                                    duration,
                                    sources,
                                    thumbnail,
                                    batchId,
                                    null,
                                    null,
                                    personalData);
                                ReflectionHelper.SetValue(m, vm => vm.Metadata!, metadata);
                            }
                            catch(VideoManifestValidationException e)
                            {
                                m.FailedValidation(e.ValidationErrors);
                            }
                        }

                        return Task.FromResult(m);
                    });

            dbContext.MapRegistry.AddModelMap<ValidationError>(
                "f555eaa8-d8e1-4f23-a402-8b9ac5930832"); //v0.3.0

            dbContext.MapRegistry.AddModelMap<VideoManifestMetadataBase>(
                "07b4aaa1-c6a2-4dc0-9342-0acd81a2c63e"); //v0.3.9

            //manifest v1
            dbContext.MapRegistry.AddModelMap<VideoManifestMetadataV1>( //v0.3.9
                "8bc43b2f-985b-443c-9a16-e9420a8a1d9d");

            dbContext.MapRegistry.AddModelMap<ThumbnailV1>(
                "91ce6fdc-b59a-46bc-9ad0-7a8608cdfa1c") //v0.3.0
                .AddFallbackSchema(mm => //dev (pre v0.3.0), published for WAM event
                {
                    mm.AutoMap();

                    // Set members with custom name.
                    mm.GetMemberMap(i => i.Blurhash).SetElementName("BlurHash");
                });

            dbContext.MapRegistry.AddModelMap<VideoSourceV1>(
                "ca9caff9-df18-4101-a362-f8f449bb2aac"); //v0.3.0

            //manifest v2
            dbContext.MapRegistry.AddModelMap<VideoManifestMetadataV2>(
                "eff75fd8-54ea-437f-862b-782a153416bc"); //v0.3.9

            dbContext.MapRegistry.AddModelMap<ThumbnailV2>(
                "36966654-d85c-455b-b870-7b49e1124e6d"); //v0.3.9

            dbContext.MapRegistry.AddModelMap<ImageSourceV2>(
                "1fbae0a8-9ee0-40f0-a8ad-21a0083fcb66"); //v0.3.9

            dbContext.MapRegistry.AddModelMap<VideoSourceV2>(
                "91231db0-aded-453e-8178-f28a0a19776a"); //v0.3.9
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
                config.AddModelMap<VideoManifest>("f7966611-14aa-4f18-92f4-8697b4927fb6", mm =>
                {
                    mm.MapMember(m => m.IsValid);
                    mm.MapMember(m => m.Manifest);

                    //*** Add again after https://etherna.atlassian.net/browse/MODM-163
                    //mm.MapMember(m => m.Duration).SetSerializer( //could be float in old documents
                    //    new NullableSerializer<long>(
                    //        new Int64Serializer(BsonType.Int64, new RepresentationConverter(false, true))));
                    //mm.MapMember(m => m.Thumbnail);
                    //mm.MapMember(m => m.Title);
                    //******
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
                config.AddModelMap<VideoManifest>("1ca89e6c-716c-4936-b7dc-908c057a3e41", mm => { });
            });
    }
}
