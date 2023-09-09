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

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class ManualReviewMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.MapRegistry.AddModelMap<ManualReviewBase>(
                "9f72b89d-ce18-417f-a2c2-bc05de28ef79", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(r => r.Author, UserMap.InformationSerializer(dbContext));
                });

            dbContext.MapRegistry.AddModelMap<ManualVideoReview>(
                "e3e734ab-d845-4ec2-8920-68956eba950d", //v0.3.0
                mm =>
                {
                    

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(r => r.Video, VideoMap.ReferenceSerializer(dbContext));
                });
        }

        /// <summary>
        /// Preview information serializer
        /// </summary>
        public static ReferenceSerializer<ManualVideoReview, string> InformationSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("1afd141e-7f00-4a28-ae3c-d12ef72757be");
                config.AddModelMap<EntityModelBase>("5d4cf56b-8b79-4f3b-81fb-b1daf32ad089", mm =>
                {
                    mm.MapMember(m => m.CreationDateTime);
                });
                config.AddModelMap<EntityModelBase<string>>("f840d7b8-8f8b-4c7a-a3c4-901558609be2", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<ManualReviewBase>("fe98e788-9faa-4faa-b7d0-e90df58bae48", mm =>
                {
                    mm.MapMember(m => m.Author).SetSerializer(UserMap.InformationSerializer(dbContext));
                    mm.MapMember(m => m.Description);
                });
                config.AddModelMap<ManualVideoReview>("2e723efe-07eb-43a5-bf0e-fa74775a677f", mm =>
                {
                    
                });
            });
    }
}
