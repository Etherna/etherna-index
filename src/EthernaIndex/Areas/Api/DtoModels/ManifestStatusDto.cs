using Etherna.EthernaIndex.Areas.Api.DtoModels.ManifestAgg;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class ManifestStatusDto
    {
        // Constructors.
        public ManifestStatusDto(
            string hash,
            bool? isValid,
            DateTime? validationTime,
            IEnumerable<ErrorDetailDto> errorDetails)
        {
            ErrorDetails = errorDetails;
            Hash = hash;
            IsValid = isValid;
            ValidationTime = validationTime;
        }

        // Properties.
        public IEnumerable<ErrorDetailDto> ErrorDetails { get; private set; }
        public string Hash { get; private set; }
        public bool? IsValid { get; private set; }
        public DateTime? ValidationTime { get; private set; }
}
}
