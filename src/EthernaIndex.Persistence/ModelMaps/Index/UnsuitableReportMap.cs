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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using System.Linq;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class UnsuitableReportMap : IModelMapsCollector
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
