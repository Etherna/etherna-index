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
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class VideoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<Video>(
                "d0c48dd8-0887-4ac5-80e5-9b08c5dc77f1", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(v => v.LastValidManifest!, VideoManifestMap.PreviewInfoSerializer(dbContext));
                    mm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContext));
                    mm.SetMemberSerializer(c => c.VideoManifests,
                        new EnumerableSerializer<VideoManifest>(
                            VideoManifestMap.ReferenceSerializer(dbContext)));
                }
                ).AddSecondarySchema(
                    "abfbd104-35ff-4429-9afc-79304a11efc0", //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with custom serializers.
                        mm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContext));
                        mm.SetMemberSerializer(c => c.VideoManifests,
                            new EnumerableSerializer<VideoManifest>(
                                VideoManifestMap.PreviewInfoSerializer(dbContext)));
                    },
                    fixDeserializedModelFunc: video =>
                    {
                        ReflectionHelper.SetValue(
                            video,
                            v => v.LastValidManifest,
                            video.VideoManifests.Where(i => i.IsValid == true)
                                                .OrderByDescending(i => i.CreationDateTime)
                                                .FirstOrDefault());
                        return Task.FromResult(video);
                    });
        }

        /// <summary>
        /// Preview information serializer
        /// </summary>
        public static ReferenceSerializer<Video, string> PreviewInfoSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("3c880345-d066-430b-8934-f8f911f52bac");
                config.AddModelMap<EntityModelBase>("9021f247-a715-4754-a5ef-b3fdd052c754", mm => { });
                config.AddModelMap<EntityModelBase<string>>("2029201a-80b1-4dfb-9038-95706a9bea90", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<Video>("cd4517e3-809d-455c-b7da-ba07c9e7280f", mm =>
                {
                    mm.MapMember(m => m.LastValidManifest).SetSerializer(VideoManifestMap.PreviewInfoSerializer(dbContext));
                });
            });

        /// <summary>
        /// Minimal reference to the entity
        /// </summary>
        public static ReferenceSerializer<Video, string> ReferenceSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("b89d81ca-1933-4a4b-844e-ce2702aaddc8");
                config.AddModelMap<EntityModelBase>("1761b6d5-71ce-4779-8d67-89de28107dc3", mm => { });
                config.AddModelMap<EntityModelBase<string>>("3455decd-e327-4d0f-a47a-ff6cded6abb7", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<Video>("d4844740-472d-48b9-b066-67ba9a2acc9b", mm => { });
            });
    }
}
