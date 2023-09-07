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

using Etherna.EthernaIndex.Domain.Models.Swarm;
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
        public VideoManifest(string manifestHash)
        {
            Manifest = new SwarmBzz(manifestHash);
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoManifest() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual bool? IsValid { get; private set; }
        public virtual SwarmBzz Manifest { get; protected set; }
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
