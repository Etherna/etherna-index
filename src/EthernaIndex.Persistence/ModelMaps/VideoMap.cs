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
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class VideoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<Video>("abfbd104-35ff-4429-9afc-79304a11efc0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(v => v.EncryptionKey!, new HexToBinaryDataSerializer());
                    cm.SetMemberSerializer(v => v.Owner, UserMap.InformationSerializer(dbContext));
                    cm.SetMemberSerializer(c => c.VideoManifest,
                        new EnumerableSerializer<VideoManifest>(
                            VideoManifestMap.InformationSerializer(dbContext, true)));
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<Video, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new(dbContext, config =>
            {
                config.UseCascadeDelete = useCascadeDelete;
                config.AddModelMapsSchema<ModelBase>("b89d81ca-1933-4a4b-844e-ce2702aaddc8");
                config.AddModelMapsSchema<EntityModelBase>("1761b6d5-71ce-4779-8d67-89de28107dc3", mm => { });
                config.AddModelMapsSchema<EntityModelBase<string>>("3455decd-e327-4d0f-a47a-ff6cded6abb7", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<Video>("d4844740-472d-48b9-b066-67ba9a2acc9b", mm =>{});
            });
    }
}
