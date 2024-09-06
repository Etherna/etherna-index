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

using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Persistence.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class BeeNetMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<PostageBatchId>( //v0.3.12
                "f9ba7202-e790-43c4-99b6-a213d1f54b82",
                customSerializer: new PostageBatchIdSerializer());
            
            dbContext.MapRegistry.AddModelMap<SwarmAddress>( //v0.3.12
                "20629304-9845-49d4-beb1-0588ae4a5abe",
                customSerializer: new SwarmAddressSerializer());
            
            dbContext.MapRegistry.AddModelMap<SwarmHash>( //v0.3.12
                "f30ef1d8-af84-4ef3-a4bb-83a4bf60e681",
                customSerializer: new SwarmHashSerializer());

            dbContext.MapRegistry.AddModelMap<SwarmUri>( //v0.3.12
                "2e563350-3127-4647-b730-8b9f44b20604",
                customSerializer: new SwarmUriSerializer());
        }
    }
}
