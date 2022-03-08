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

using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Fields.
        private List<VideoManifest> _videoManifests = new();

        // Constructors and dispose.
        public Video(User owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Owner.AddVideo(this);
        }

        protected Video() { }

        public override void DisposeForDelete()
        {
            //owner channel
            Owner.RemoveVideo(this);

            base.DisposeForDelete();
        }

        // Properties.
        public virtual bool Freezed { get; set; }
        public virtual VideoManifest? LastValidManifest => _videoManifests.Where(i => i.IsValid == true)
                                                                          .OrderByDescending(i => i.CreationDateTime)
                                                                          .FirstOrDefault();
        public virtual User Owner { get; protected set; } = default!;
        public virtual string? Title => LastValidManifest?.Title;
        public virtual long TotDownvotes { get; set; }
        public virtual long TotUpvotes { get; set; }
        public virtual IEnumerable<VideoManifest> VideoManifests
        {
            get => _videoManifests;
            protected set => _videoManifests = new List<VideoManifest>(value ?? new List<VideoManifest>());
        }

        // Methods.

        [PropertyAlterer(nameof(VideoManifests))]
        public virtual void AddManifest(VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            if (_videoManifests.Any(i => i.Manifest.Hash == videoManifest.Manifest.Hash))
            {
                var ex = new InvalidOperationException("AddManifest duplicate");
                ex.Data.Add("ManifestHash", videoManifest.Manifest.Hash);
                throw ex;
            }

            _videoManifests.Add(videoManifest);
        }

        [PropertyAlterer(nameof(VideoManifests))]
        public virtual bool RemoveManifest(VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            return _videoManifests.Remove(videoManifest);
        }

    }
}
