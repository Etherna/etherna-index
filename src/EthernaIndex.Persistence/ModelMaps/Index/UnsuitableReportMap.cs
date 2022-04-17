using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Mapping;
using System.Linq;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    class UnsuitableReportMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            // register class maps.
            dbContext.SchemaRegistry.AddModelMapsSchema<UnsuitableReportBase>(
                "d658ffcf-91ea-4e5e-b163-92eabb5490cc", //dev (pre v0.3.0), published for WAM event
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(c => c.ReporterAuthor, UserMap.InformationSerializer(dbContext));
                });

            dbContext.SchemaRegistry.AddModelMapsSchema<UnsuitableVideoReport>(
                "39e398d3-3199-43e1-8147-2876b534fbec", //v0.3.0
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(c => c.Video, VideoMap.ReferenceSerializer(dbContext));
                    mm.SetMemberSerializer(c => c.VideoManifest, VideoManifestMap.BasicInformationSerializer(dbContext));
                })
                .AddSecondaryMap(new ModelMap<UnsuitableVideoReport>(
                    "91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d", //dev (pre v0.3.0), published for WAM event
                    new BsonClassMap<UnsuitableVideoReport>(mm =>
                    {
                        mm.AutoMap();

                        // Set members with custom serializers.
                        mm.SetMemberSerializer(c => c.VideoManifest, VideoManifestMap.BasicInformationSerializer(dbContext));
                    }),
                    fixDeserializedModelFunc: async model =>
                    {
                        var indexDbContext = (IIndexDbContext)dbContext;
                        var video = await indexDbContext.Videos.TryFindOneAsync(
                            v => v.VideoManifests.Any(m => m.Id == model.VideoManifest.Id));
                        ReflectionHelper.SetValue(model, m => m.Video, video);
                        return model;
                    }));
        }
    }
}
