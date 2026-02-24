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
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.Sdk.Tools.Video.Models;
using Moq;
using System;
using System.Linq;
using Xunit;
using VideoManifest = Etherna.EthernaIndex.Domain.Models.VideoAgg.VideoManifest;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoTest
    {
        // Fields.
        private readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        private readonly SwarmReference manifestReference = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        private readonly SwarmReference secondManifestReference = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
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
            // Assert.
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(0, video.TotDownvotes);
            Assert.NotNull(video.Owner);
            Assert.Empty(video.VideoManifests);
        }

        [Fact]
        public void AddVideo_ExeptionWhenDuplicated()
        {
            var videoManifest = CreateManifest(secondManifestReference, true);
            var duplicatedVideoManifest = CreateManifest(secondManifestReference, true);
            video.AddManifest(videoManifest);

            // Action.
            Assert.Throws<InvalidOperationException>(() => video.AddManifest(duplicatedVideoManifest));
        }

        [Fact]
        public void AddVideo_WhenIsValidated()
        {
            // Arrange.
            var videoManifestValid = CreateManifest(manifestReference, true);
            var videoManifestNotValid = CreateManifest(secondManifestReference, false);

            // Action.
            video.AddManifest(videoManifestValid);
            video.AddManifest(videoManifestNotValid);

            // Assert.
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == manifestReference);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == secondManifestReference);
        }

        // Helpers.
        private VideoManifest CreateManifest(SwarmReference reference, bool valid)
        {
            var videoManifest = new VideoManifest(reference);

            if (valid)
                videoManifest.SucceededValidation(new VideoManifestMetadataV1(
                    "FeddTopicTest",
                    "DescTest",
                    1,
                    [new VideoSourceV1(null, "1080", SwarmReference.PlainZero, 4)],
                    null,
                    null,
                    null,
                    null,
                    null));
            else
                videoManifest.FailedValidation([new(ValidationErrorType.Unknown, "test")]);

            return videoManifest;
        }
    }
}
