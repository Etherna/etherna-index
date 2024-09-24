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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using System.Linq;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class UnsuitableReportMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.MapRegistry.AddModelMap<UnsuitableReportBase>(
                "d658ffcf-91ea-4e5e-b163-92eabb5490cc", //dev (pre v0.3.0), published for WAM event
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(c => c.ReporterAuthor, UserMap.InformationSerializer(dbContext));
                });

            dbContext.MapRegistry.AddModelMap<UnsuitableVideoReport>(
                "39e398d3-3199-43e1-8147-2876b534fbec", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(c => c.Video, VideoMap.ReferenceSerializer(dbContext));
                    mm.SetMemberSerializer(c => c.VideoManifest, VideoManifestMap.ReferenceSerializer(dbContext));
                })
                .AddSecondarySchema(
                    "91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d", //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with custom serializers.
                        mm.SetMemberSerializer(c => c.VideoManifest, VideoManifestMap.PreviewInfoSerializer(dbContext));
                    },
                    fixDeserializedModelFunc: async model =>
                    {
                        var indexDbContext = (IIndexDbContext)dbContext;
                        var video = await indexDbContext.Videos.TryFindOneAsync(
                            v => v.VideoManifests.Any(m => m.Id == model.VideoManifest.Id));
                        ReflectionHelper.SetValue(model, m => m.Video, video);
                        return model;
                    });
        }
    }
}
