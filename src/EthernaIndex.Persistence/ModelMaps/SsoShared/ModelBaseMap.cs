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
using Etherna.MongoDB.Bson.Serialization.IdGenerators;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.SsoShared
{
    internal sealed class ModelBaseMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.MapRegistry.AddModelMap<ModelBase>(
                "d517f32d-cc45-4d21-8a99-27dca658bde5"); //from sso v0.3.0
            dbContext.MapRegistry.AddModelMap<EntityModelBase>(
                "4c17bb54-af84-4a21-83ae-cb1050b721f5"); //from sso v0.3.0
            dbContext.MapRegistry.AddModelMap<EntityModelBase<string>>(
                "e5e834e0-30cc-42a8-a1a2-9d5c79d35485", //from sso v0.3.0
                modelMap =>
                {
                    modelMap.AutoMap();

                    // Set Id representation.
                    modelMap.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId))
                                        .SetIdGenerator(new StringObjectIdGenerator());
                });
        }
    }
}
