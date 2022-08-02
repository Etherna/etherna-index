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
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class User : EntityModelBase<string>
    {
        // Fields.
        private List<Video> _videos = new();

        // Constructors.
        public User(UserSharedInfo sharedInfo)
        {
            if (sharedInfo is null)
                throw new ArgumentNullException(nameof(sharedInfo));

            SharedInfoId = sharedInfo.Id;
        }
        protected User() { }

        // Properties.
        /* SharedInfo is encapsulable with resolution of https://etherna.atlassian.net/browse/MODM-101.
         * With encapsulation we can expose also EtherAddress and EtherPreviousAddresses properties
         * pointing to SharedInfo internal property.
         */
        //protected virtual SharedUserInfo SharedInfo { get; set; }
        public virtual string SharedInfoId { get; protected set; } = default!;

        public virtual IEnumerable<Video> Videos
        {
            get => _videos;
            protected set => _videos = new List<Video>(value ?? Array.Empty<Video>());
        }

        // Methods.
        [PropertyAlterer(nameof(Videos))]
        protected internal virtual void AddVideo(Video video)
        {
            if (!_videos.Contains(video))
                _videos.Add(video);
        }

        [PropertyAlterer(nameof(Videos))]
        protected internal virtual void RemoveVideo(Video video)
        {
            _videos.Remove(video);
        }
    }
}
