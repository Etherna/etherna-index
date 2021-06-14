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
using System;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class UserTest
    {
        // Consts.
        private const string UserAddress = "0xFb6916095cA1Df60bb79ce92cE3EA74c37C5d359";
        private readonly SwarmContentHash VideoManifestHash = new SwarmContentHash("33f1ea45b3404d1691911729a5dd618216bbd2031c9bf1459d4f4542fb13e067");

        // Fields.
        private readonly User user;
        private readonly Video video;

        // Constructors.
        public UserTest()
        {
            user = new User(UserAddress);
            video = new Video(null, EncryptionType.Plain, VideoManifestHash, user);
        }

        // Tests.
        [Fact]
        public void AddVideo()
        {
            // Action.
            user.AddVideo(video);

            // Assert.
            Assert.Contains(video, user.Videos);
            Assert.Equal(user, video.Owner);
        }

        [Fact]
        public void InvalidAddress()
        {
            // Assert.
            Assert.Throws<ArgumentNullException>(() => new User(null));
            Assert.Throws<ArgumentException>(() => new User("ImNotAnAddress"));
        }

        [Fact]
        public void RemoveVideo()
        {
            // Setup.
            user.AddVideo(video);

            // Action.
            user.RemoveVideo(video);

            // Assert.
            Assert.DoesNotContain(video, user.Videos);
        }
    }
}
