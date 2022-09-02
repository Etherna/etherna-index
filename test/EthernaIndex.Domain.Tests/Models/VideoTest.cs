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

using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoTest
    {
        // Fields.
        private readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        private readonly string manifestHash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        private readonly string secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        //private readonly string videoId = "42f44721db94b734c";
        private readonly User owner;
        private readonly Mock<UserSharedInfo> userSharedInfoMock = new();
        private readonly Video video;

        // Constructors.
        public VideoTest()
        {
            userSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            owner = new User(userSharedInfoMock.Object);
            video = new Video(owner);
        }

        // Tests.
        [Fact]
        public void Create_Video()
        {
            // Arrange.

            // Action.

            // Assert.
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(0, video.TotDownvotes);
            Assert.NotNull(video.Owner);
            Assert.Empty(video.VideoManifests);
        }

        [Fact]
        public void AddVideo_ExeptionWhenDuplicated()
        {
            var videoManifest = CreateManifest(secondManifestHash, true);
            var duplicatedVideoManifest = CreateManifest(secondManifestHash, true);
            video.AddManifest(videoManifest);

            // Action.
            Assert.Throws<InvalidOperationException>(() => video.AddManifest(duplicatedVideoManifest));
        }

        [Fact]
        public void AddVideo_WhenIsValidated()
        {
            // Arrange.
            var videoManifestValid = CreateManifest(manifestHash, true);
            var videoManifestNotValid = CreateManifest(secondManifestHash, false);

            // Action.
            video.AddManifest(videoManifestValid);
            video.AddManifest(videoManifestNotValid);

            // Assert.
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == secondManifestHash);
        }

        // Helpers.
        private VideoManifest CreateManifest(string hash, bool valid)
        {
            var videoManifest = new VideoManifest(hash);
            var title = "FeddTopicTest";
            var desc = "DescTest";
            var original = "OriginalTest";
            var duration = 1;
            var videoSources = new List<VideoSource> {
                new VideoSource(1, "10801", "reff1", 4),
                new VideoSource(2, "321", "reff2", 100) };

            if (valid)
                videoManifest.SucceededValidation(
                    null,
                    desc,
                    duration,
                    original,
                    videoSources,
                    null,
                    title);
            else
                videoManifest.FailedValidation(new List<ErrorDetail> { new ErrorDetail(ValidationErrorType.Unknown, "test") });

            return videoManifest;
        }
    }
}
