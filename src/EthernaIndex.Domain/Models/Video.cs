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

using Etherna.EthernaIndex.Domain.Events;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Fields.
        private VideoManifest? _lastValidManifest;
        private List<VideoManifest> _videoManifests = new();

        // Constructors and dispose.
        public Video(User owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        protected Video() { }

        // Properties.
        public virtual bool IsFrozen { get; set; }
        public virtual VideoManifest? LastValidManifest
        {
            get => _lastValidManifest;
            protected set
            {
                //old manifests could be deserialized from db as valid, even if they aren't with current rules
                if (value?.IsValid == true)
                    _lastValidManifest = value;
            }
        }
        public virtual User Owner { get; protected set; } = default!;
        public virtual long TotDownvotes { get; set; }
        public virtual long TotUpvotes { get; set; }
        public virtual IEnumerable<VideoManifest> VideoManifests
        {
            get => _videoManifests;
            protected set => _videoManifests = new List<VideoManifest>(value ?? new List<VideoManifest>());
        }

        // Methods.
        [PropertyAlterer(nameof(LastValidManifest))]
        [PropertyAlterer(nameof(VideoManifests))]
        public virtual void AddManifest(VideoManifest videoManifest)
        {
            ArgumentNullException.ThrowIfNull(videoManifest, nameof(videoManifest));
            if (IsFrozen)
                throw new InvalidOperationException("Video is frozen");

            if (_videoManifests.Any(i => i.ManifestHash == videoManifest.ManifestHash))
            {
                var ex = new InvalidOperationException("AddManifest duplicate");
                ex.Data.Add("ManifestHash", videoManifest.ManifestHash.ToString());
                throw ex;
            }

            _videoManifests.Add(videoManifest);

            UpdateLastValidManifest();
        }

        [PropertyAlterer(nameof(LastValidManifest))]
        public virtual void FailedManifestValidation(
            VideoManifest manifest,
            IEnumerable<ValidationError> validationErrors)
        {
            ArgumentNullException.ThrowIfNull(manifest, nameof(manifest));
            ArgumentNullException.ThrowIfNull(validationErrors, nameof(validationErrors));

            if (!VideoManifests.Contains(manifest))
            {
                var ex = new InvalidOperationException("The manifest is not owned by this video");
                ex.Data.Add("ManifestHash", manifest.ManifestHash.ToString());
                throw ex;
            }

            manifest.FailedValidation(validationErrors);

            UpdateLastValidManifest();
        }

        [PropertyAlterer(nameof(LastValidManifest))]
        [PropertyAlterer(nameof(VideoManifests))]
        public virtual bool RemoveManifest(VideoManifest videoManifest)
        {
            ArgumentNullException.ThrowIfNull(videoManifest, nameof(videoManifest));
            if (IsFrozen)
                throw new InvalidOperationException("Video is frozen");

            var result = _videoManifests.Remove(videoManifest);

            if (result)
                UpdateLastValidManifest();

            return result;
        }

        [PropertyAlterer(nameof(IsFrozen))]
        [PropertyAlterer(nameof(LastValidManifest))]
        [PropertyAlterer(nameof(VideoManifests))]
        public virtual void SetAsUnsuitable()
        {
            IsFrozen = true;
            _lastValidManifest = null;
            _videoManifests.Clear();
            AddEvent(new VideoModeratedEvent(this));
        }

        [PropertyAlterer(nameof(LastValidManifest))]
        public virtual void SucceededManifestValidation(
            VideoManifest manifest,
            VideoManifestMetadataBase metadata)
        {
            ArgumentNullException.ThrowIfNull(manifest, nameof(manifest));
            ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

            if (!VideoManifests.Contains(manifest))
            {
                var ex = new InvalidOperationException("The manifest is not owned by this video");
                ex.Data.Add("ManifestHash", manifest.ManifestHash.ToString());
                throw ex;
            }

            manifest.SucceededValidation(metadata);

            UpdateLastValidManifest();

            // Raise event.
            AddEvent(new ManifestSuccessfulValidatedEvent(this, manifest));
        }

        // Helpers.
        private void UpdateLastValidManifest() =>
            _lastValidManifest = VideoManifests
                .Where(i => i.IsValid == true)
                .OrderByDescending(i => i.CreationDateTime)
                .FirstOrDefault();
    }
}
