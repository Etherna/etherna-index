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
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<User>(
                "9a2d9664-31d5-4394-9a20-c8789cf0600d", //v0.3.0
                mm =>
                {
                    mm.AutoMap();
                })
                .AddSecondarySchema("a547abdc-420c-41f9-b496-e6cf704a3844"); //dev (pre v0.3.0), published for WAM event
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<User, string> InformationSerializer(IDbContext dbContext) =>
            new(dbContext, config =>
            {
                config.AddModelMap<ModelBase>("f2b68f90-0851-40fc-a9af-556458f85662");
                config.AddModelMap<EntityModelBase>("1401ce64-0eb9-4f64-b9b2-cd570934268b", mm => { });
                config.AddModelMap<EntityModelBase<string>>("3fabed81-1e86-4183-86dc-b875f9a940ac", mm =>
                {
                    mm.MapIdMember(m => m.Id);
                    mm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
                config.AddModelMap<User>("caa0968f-4493-485b-b8d0-bc40942e8684", mm =>
                {
                    mm.MapMember(u => u.SharedInfoId);
                });
            });
    }
}
