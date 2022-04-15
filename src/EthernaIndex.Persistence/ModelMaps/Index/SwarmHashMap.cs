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

using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class SwarmHashMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<SwarmContentBase>(
                "813cf8b6-df89-4a5d-8c2d-a9a9e08b6922"); //v0.3.0
            dbContext.SchemaRegistry.AddModelMapsSchema<SwarmBytes>(
                "aa2fac3c-7362-4c1c-96ec-dafaa6327322"); //v0.3.0
            dbContext.SchemaRegistry.AddModelMapsSchema<SwarmBzz>(
                "27edd50c-dd67-44d8-84ea-1eedcfe481e8"); //v0.3.0
            dbContext.SchemaRegistry.AddModelMapsSchema<SwarmSoc>(
                "2feca50a-3009-4fe1-a9d3-b9549de29d1d"); //v0.3.0
        }
    }
}
