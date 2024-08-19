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
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;

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
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(r => r.Video, VideoMap.ReferenceSerializer(dbContext));
                });
        }
    }
}
