using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.ValidationResults;
using Etherna.EthernaIndex.Services.Interfaces;
using EthernaIndex.Swarm;
using EthernaIndex.Swarm.DtoModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.ModelValidators
{
    public class MetadataVideoValidator : IMetadataVideoValidator
    {
        // Fields.
        private readonly IIndexContext indexContext;
        private readonly ISwarmService swarmService;

        // Constructors.
        public MetadataVideoValidator(
            IIndexContext indexContext,
            ISwarmService swarmService)
        {
            this.indexContext = indexContext;
            this.swarmService = swarmService;
        }

        // Methods.
        public async Task<bool> CheckManifestAsync(string manifestHash)
        {
            var validationResult = await indexContext.ValidationResults.FindOneAsync(u => u.Id == manifestHash);

            MetadataVideoDto? metadataDto;
            var validationErrors = new Dictionary<ValidationError, string>();

            //Get Metadata and check JsonConvert
            try
            {
                metadataDto = await swarmService.GetMetadataVideoAsync(manifestHash);
            }
            catch (MetadataVideoException ex)
            {
                metadataDto = null;
                validationErrors.Add(ValidationError.JsonConvert, ex.Message);
            }

            //Check Title
            if (metadataDto is not null &&
                string.IsNullOrWhiteSpace(metadataDto.Title))
                    validationErrors.Add(ValidationError.MissingTitle, ValidationError.MissingTitle.ToString());


            //Check Video Format
            var validationVideoError = CheckVideoFormat(metadataDto);
            if (validationVideoError is not null)
            {
                validationErrors.Add(validationVideoError.Value, validationVideoError.Value.ToString());
            }

            validationResult.SetResult(validationErrors);
            await indexContext.SaveChangesAsync();

            return true;
        }

        private ValidationError? CheckVideoFormat(MetadataVideoDto? metadataDto)
        {
            if (metadataDto is null)
            {
                return null;
            }

            //TODO get rules for validation video format

            return null;
        }

    }
}
