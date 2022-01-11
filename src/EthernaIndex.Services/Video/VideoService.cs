using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Interfaces;
using Etherna.EthernaIndex.Domain.Models.Meta;
using Etherna.EthernaIndex.Services.Swarm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Video
{
    public class VideoService : IVideoService
    {
        // Fields.
        private readonly ISwarmService swarmService;
        private readonly IMetadataVideoValidator metadataVideoValidator;
        private readonly IIndexContext indexContext;

        // Constructors.
        public VideoService(
            ISwarmService swarmService,
            IMetadataVideoValidator metadataVideoValidator,
            IIndexContext indexContext)
        {
            this.swarmService = swarmService;
            this.metadataVideoValidator = metadataVideoValidator;
            this.indexContext = indexContext;
        }

        // Methods.

        public async Task ValidateMetadataAsync(string manifestHash)
        {
            var metadataModel = await indexContext.MetadataVideos.FindOneAsync(u => u.Id == manifestHash);

            if (metadataModel is null)
            {
                var metadataDto = await swarmService.GetMetadataVideoAsync(manifestHash);
                metadataModel = await MetadataVideo.CreateMetadataVideoAsync(metadataDto, metadataVideoValidator);
                await indexContext.MetadataVideos.CreateAsync(metadataModel);
            }
            else
            {
                await metadataModel.CheckValidationAsync(metadataVideoValidator);
                await indexContext.SaveChangesAsync();
            }
        }
    }
}
