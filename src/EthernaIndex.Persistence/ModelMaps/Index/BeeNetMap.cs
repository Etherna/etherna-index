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
            dbContext.MapRegistry.AddCustomSerializerMap<PostageBatchId>( //v0.3.12
                new PostageBatchIdSerializer());
            
            dbContext.MapRegistry.AddCustomSerializerMap<SwarmAddress>( //v0.3.12
                new SwarmAddressSerializer());
            
            dbContext.MapRegistry.AddCustomSerializerMap<SwarmHash>( //v0.3.12
                new SwarmHashSerializer());

            dbContext.MapRegistry.AddCustomSerializerMap<SwarmUri>( //v0.3.12
                new SwarmUriSerializer());
        }
    }
}
