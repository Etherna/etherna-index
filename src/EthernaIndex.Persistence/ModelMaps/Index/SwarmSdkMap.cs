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

using Etherna.EthernaIndex.Persistence.Serializers;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Serialization;
using Etherna.SwarmSdk.Models;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class SwarmSdkMap : IModelMapsCollector
    {
        public void Register(IDbContextEngine dbContextEngine)
        {
            dbContextEngine.MapRegistry.AddCustomSerializerMap<EthAddress>( //v0.3.15
                new EthAddressSerializer());
            
            dbContextEngine.MapRegistry.AddCustomSerializerMap<PostageBatchId>( //v0.3.12
                new PostageBatchIdSerializer());
            
            dbContextEngine.MapRegistry.AddCustomSerializerMap<SwarmAddress>( //v0.3.12
                new SwarmAddressSerializer());
            
            dbContextEngine.MapRegistry.AddCustomSerializerMap<SwarmReference>( //v0.3.15
                new SwarmReferenceSerializer());

            dbContextEngine.MapRegistry.AddCustomSerializerMap<SwarmUri>( //v0.3.12
                new SwarmUriSerializer());
        }
    }
}
