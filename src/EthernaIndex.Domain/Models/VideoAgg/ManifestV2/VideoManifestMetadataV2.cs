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

using System.Collections.Generic;

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
            long createdAt,
            long? updatedAt,
            string? personalData)
        {
            AspectRatio = aspectRatio;
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
        public virtual long CreatedAt { get; protected set; }
        public virtual string Description { get; protected set; }
        public virtual long Duration { get; protected set; }
        public virtual string? PersonalData { get; protected set; }
        public virtual IEnumerable<VideoSourceV2> Sources
        {
            get => _sources;
            protected set => _sources = [..value ?? new List<VideoSourceV2>()];
        }
        public virtual ThumbnailV2? Thumbnail { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual long? UpdatedAt { get; protected set; }
    }
}
