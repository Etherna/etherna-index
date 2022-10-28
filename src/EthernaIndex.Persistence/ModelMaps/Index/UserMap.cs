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
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<User>(
                "9a2d9664-31d5-4394-9a20-c8789cf0600d", //v0.3.0
                mm =>
                {
                    mm.AutoMap();
                })
                .AddSecondaryMap(new MongODM.Core.Serialization.Mapping.ModelMap<User>(
                    "a547abdc-420c-41f9-b496-e6cf704a3844", //dev (pre v0.3.0), published for WAM event
                    new MongoDB.Bson.Serialization.BsonClassMap<User>(mm =>
                    {
                        mm.AutoMap();
                    })));
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<User, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new(dbContext, config =>
            {
                config.UseCascadeDelete = useCascadeDelete;
                config.AddModelMapsSchema<ModelBase>("f2b68f90-0851-40fc-a9af-556458f85662");
                config.AddModelMapsSchema<EntityModelBase>("1401ce64-0eb9-4f64-b9b2-cd570934268b", mm => { });
                config.AddModelMapsSchema<EntityModelBase<string>>("3fabed81-1e86-4183-86dc-b875f9a940ac", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMapsSchema<User>("caa0968f-4493-485b-b8d0-bc40942e8684", mm =>
                {
                    mm.MapMember(u => u.SharedInfoId);
                });
            });
    }
}
