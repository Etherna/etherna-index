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

using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class SwarmHashMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<SwarmContentBase>(
                "813cf8b6-df89-4a5d-8c2d-a9a9e08b6922"); //v0.3.0

            dbContext.MapRegistry.AddModelMap<SwarmBytes>(
                "aa2fac3c-7362-4c1c-96ec-dafaa6327322"); //v0.3.0

            dbContext.MapRegistry.AddModelMap<SwarmBzz>(
                "27edd50c-dd67-44d8-84ea-1eedcfe481e8"); //v0.3.0

            dbContext.MapRegistry.AddModelMap<SwarmSoc>(
                "2feca50a-3009-4fe1-a9d3-b9549de29d1d"); //v0.3.0
        }
    }
}
