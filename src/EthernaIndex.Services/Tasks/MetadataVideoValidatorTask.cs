//   Copyright 2020-present Etherna Sagl
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
using Etherna.EthernaIndex.Domain.Models.ValidationResults;
using Etherna.EthernaIndex.Services.Interfaces;
using EthernaIndex.Swarm;
using EthernaIndex.Swarm.DtoModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public class MetadataVideoValidatorTask : IMetadataVideoValidatorTask
    {
        // Fields.
        private readonly IIndexContext indexContext;
        private readonly ISwarmService swarmService;

        // Constructors.
        public MetadataVideoValidatorTask(
            IIndexContext indexContext,
            ISwarmService swarmService)
        {
            this.indexContext = indexContext;
            this.swarmService = swarmService;
        }

        // Methods.
        public async Task RunAsync(string manifestHash)
        {
            var validationResult = await indexContext.ValidationResults.FindOneAsync(u => u.ManifestHash.Hash == manifestHash);

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

            // Complete task.
            await indexContext.SaveChangesAsync().ConfigureAwait(false);
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
