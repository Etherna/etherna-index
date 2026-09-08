// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Extensions;
using Etherna.Scrinium.Core.Options;
using Etherna.Scrinium.Core.Serialization;
using Etherna.Scrinium.Core.Serialization.Serializers;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class VideoMap : IModelMapsCollector
    {
        public void Register(IDbContextEngine dbContextEngine)
        {
            dbContextEngine.MapRegistry.AddModelMap<Video>(
                "d0c48dd8-0887-4ac5-80e5-9b08c5dc77f1", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(v => v.LastValidManifest!, VideoManifestMap.PreviewInfoSerializer(dbContextEngine, OriginDeleteMode.RemoveReference));
                    mm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContextEngine));
                    mm.SetMemberSerializer(c => c.VideoManifests,
                        new EnumerableSerializer<VideoManifest>(
                            VideoManifestMap.ReferenceSerializer(dbContextEngine, OriginDeleteMode.RemoveReference)));
                }
                ).AddSecondarySchema(
                    "abfbd104-35ff-4429-9afc-79304a11efc0", //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with custom serializers.
                        mm.SetMemberSerializer(v => v.LastValidManifest!, VideoManifestMap.PreviewInfoSerializer(dbContextEngine, OriginDeleteMode.RemoveReference));
                        mm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContextEngine));
                        mm.SetMemberSerializer(c => c.VideoManifests,
                            new EnumerableSerializer<VideoManifest>(
                                VideoManifestMap.PreviewInfoSerializer(dbContextEngine, OriginDeleteMode.RemoveReference)));
                    },
                    fixDeserializedModelFunc: (_, video) =>
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
        /// <param name="originDelete">Reaction of the referencing documents to the video deletion</param>
        public static ReferenceSerializer<Video, string> PreviewInfoSerializer(IDbContextEngine dbContextEngine, OriginDeleteMode originDelete) =>
            new(dbContextEngine, config =>
            {
                config.OriginDelete = originDelete;
                config.AddModelMap<ModelBase>("3c880345-d066-430b-8934-f8f911f52bac");
                config.AddModelMap<EntityModelBase>("9021f247-a715-4754-a5ef-b3fdd052c754", mm => { });
                config.AddModelMap<EntityModelBase<string>>("2029201a-80b1-4dfb-9038-95706a9bea90", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<Video>("cd4517e3-809d-455c-b7da-ba07c9e7280f", mm =>
                {
                    mm.MapMember(m => m.LastValidManifest).SetSerializer(VideoManifestMap.PreviewInfoSerializer(dbContextEngine, OriginDeleteMode.RemoveReference));
                });
            });

        /// <summary>
        /// Minimal reference to the entity
        /// </summary>
        /// <param name="originDelete">Reaction of the referencing documents to the video deletion</param>
        public static ReferenceSerializer<Video, string> ReferenceSerializer(IDbContextEngine dbContextEngine, OriginDeleteMode originDelete) =>
            new(dbContextEngine, config =>
            {
                config.OriginDelete = originDelete;
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
