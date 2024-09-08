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

using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class VideoManifest : EntityModelBase<string>
    {
        // Consts.
        public const int CurrentDescriptionMaxLength = VideoManifestMetadataV2.DescriptionMaxLength;
        public const int CurrentPersonalDataMaxLength = VideoManifestMetadataV2.PersonalDataMaxLength;
        public const int CurrentTitleMaxLength = VideoManifestMetadataV2.TitleMaxLength;

        // Fields.
        private List<ValidationError> _validationErrors = new();

        // Constructors.
        public VideoManifest(SwarmHash manifestHash)
        {
            ManifestHash = manifestHash;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoManifest() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual bool? IsValid { get; private set; }
        public virtual SwarmHash ManifestHash { get; protected set; }
        public virtual VideoManifestMetadataBase? Metadata { get; protected set; }
        public virtual IEnumerable<ValidationError> ValidationErrors
        {
            get => _validationErrors;
            protected set => _validationErrors = new List<ValidationError>(value ?? Array.Empty<ValidationError>());
        }
        public virtual DateTime? ValidationTime { get; private set; }

        // Public methods.
        public virtual string? TryGetDescription() =>
            Metadata switch
            {
                null => null,
                VideoManifestMetadataV1 metadataV1 => metadataV1.Description,
                VideoManifestMetadataV2 metadataV2 => metadataV2.Description,
                _ => throw new InvalidOperationException()
            };

        public virtual string? TryGetTitle() =>
            Metadata switch
            {
                null => null,
                VideoManifestMetadataV1 metadataV1 => metadataV1.Title,
                VideoManifestMetadataV2 metadataV2 => metadataV2.Title,
                _ => throw new InvalidOperationException()
            };

        // Internal methods.
        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(Metadata))]
        [PropertyAlterer(nameof(ValidationErrors))]
        [PropertyAlterer(nameof(ValidationTime))]
        internal virtual void FailedValidation(IEnumerable<ValidationError> validationErrors)
        {
            IsValid = false;
            Metadata = null;
            _validationErrors.AddRange(validationErrors);
            ValidationTime = DateTime.UtcNow;
        }

        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(Metadata))]
        [PropertyAlterer(nameof(ValidationErrors))]
        [PropertyAlterer(nameof(ValidationTime))]
        internal virtual void SucceededValidation(VideoManifestMetadataBase metadata)
        {
            IsValid = true;
            Metadata = metadata;
            _validationErrors.Clear();
            ValidationTime = DateTime.UtcNow;
        }
    }
}
