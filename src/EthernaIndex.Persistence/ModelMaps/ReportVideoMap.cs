using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
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

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class ReportVideoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<ReportVideo>("91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(c => c.Video, VideoMap.InformationSerializer(dbContext, true));
                });
        }
    }
}
