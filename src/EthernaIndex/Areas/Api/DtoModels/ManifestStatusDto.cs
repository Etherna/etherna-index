using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class ManifestStatusDto
    {
        // Constructors.
        public ManifestStatusDto(VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            ErrorDetails = videoManifest.ErrorValidationResults
                .Select(i => new ErrorDetailDto(i.ErrorMessage, i.ErrorType));
            Hash = videoManifest.Manifest.Hash;
            IsValid = videoManifest.IsValid;
            ValidationTime = videoManifest.ValidationTime;
        }

        // Properties.
        public IEnumerable<ErrorDetailDto> ErrorDetails { get; private set; }
        public string Hash { get; private set; }
        public bool? IsValid { get; private set; }
        public DateTime? ValidationTime { get; private set; }
}
}
