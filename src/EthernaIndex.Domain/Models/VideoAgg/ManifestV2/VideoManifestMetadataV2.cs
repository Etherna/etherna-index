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

using Etherna.EthernaIndex.Domain.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2
{
    public class VideoManifestMetadataV2 : VideoManifestMetadataBase
    {
        // Consts.
        public const int DescriptionMaxLength = 5000;
        public const int PersonalDataMaxLength = 200;
        public const int TitleMaxLength = 200;

        // Fields.
        private List<VideoSourceV2> _sources = new();

        // Constructors.
        public VideoManifestMetadataV2(
            string title,
            string description,
            long duration,
            IEnumerable<VideoSourceV2> sources,
            ThumbnailV2? thumbnail,
            float aspectRatio,
            string batchId,
            long createdAt,
            long? updatedAt,
            string? personalData)
        {
            // Validate args.
            var validationErrors = new List<ValidationError>();

            //title
            if (string.IsNullOrWhiteSpace(title))
                validationErrors.Add(new ValidationError(ValidationErrorType.MissingTitle));
            else if (title.Length > int.Max(TitleMaxLength, VideoManifest.CurrentTitleMaxLength))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidTitle, "Title is too long"));

            //description
            if (description is null)
                validationErrors.Add(new ValidationError(ValidationErrorType.MissingDescription));
            else if (description.Length > int.Max(DescriptionMaxLength, VideoManifest.CurrentDescriptionMaxLength))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidDescription, "Description is too long"));

            //duration
            if (duration == 0)
                validationErrors.Add(new ValidationError(ValidationErrorType.MissingDuration));

            //video sources
            if (sources is null || !sources.Any())
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidVideoSource, "Missing sources"));

            //aspect ratio
            if (aspectRatio <= 0)
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidAspectRatio));

            //batchId
            if (string.IsNullOrWhiteSpace(batchId))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidBatchId, "Missing batch Id"));

            //createdAt
            if (createdAt <= 0)
                validationErrors.Add(new ValidationError(ValidationErrorType.MissingManifestCreationTime));

            //personal data
            if (personalData is not null &&
                personalData.Length > int.Max(PersonalDataMaxLength, VideoManifest.CurrentPersonalDataMaxLength))
                validationErrors.Add(new ValidationError(ValidationErrorType.InvalidPersonalData, "Personal data is too long"));

            // Throws validation exception.
            if (validationErrors.Any())
                throw new VideoManifestValidationException(validationErrors);

            // Assign properties.
            AspectRatio = aspectRatio;
            BatchId = batchId;
            CreatedAt = createdAt;
            Description = description!;
            Duration = duration;
            PersonalData = personalData;
            Sources = sources!;
            Thumbnail = thumbnail;
            Title = title;
            UpdatedAt = updatedAt;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoManifestMetadataV2() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        //from v2.0
        public virtual float AspectRatio { get; protected set; }
        public virtual string BatchId { get; protected set; }
        public virtual long CreatedAt { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual long Duration { get; protected set; }
        public virtual string? PersonalData { get; protected set; }
        public virtual IEnumerable<VideoSourceV2> Sources
        {
            get => _sources;
            protected set => _sources = new List<VideoSourceV2>(value ?? new List<VideoSourceV2>());
        }
        public virtual ThumbnailV2? Thumbnail { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual long? UpdatedAt { get; protected set; }
    }
}
