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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Exceptions;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.EthernaIndex.Swarm;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EthernaIndex.Services.Tests.Tasks
{
    public class VideoManifestValidatorTaskTest
    {
        // Fields.
        private readonly IVideoManifestValidatorTask videoManifestValidatorTask;
        private readonly string manifestHash = "1a345a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        private readonly string videoId = "videoId";
        private readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        private readonly Mock<UserSharedInfo> userSharedInfoMock = new();
        private readonly Video video;
        private readonly VideoManifest videoManifest;
        private readonly Mock<IIndexDbContext> indexContext;
        private readonly Mock<ILogger<VideoManifestValidatorTask>> loggerMock;
        private readonly Mock<ISwarmService> swarmService;

        // Constructor.
        public VideoManifestValidatorTaskTest()
        {
            userSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            var owner = new User(userSharedInfoMock.Object);
            video = new Video(owner);
            videoManifest = new VideoManifest(manifestHash);
            video.AddManifest(videoManifest);

            loggerMock = new Mock<ILogger<VideoManifestValidatorTask>>();
            swarmService = new Mock<ISwarmService>();

            // Mock Db Data.
            indexContext = new Mock<IIndexDbContext>();
            indexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(videoManifest);
            indexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);

            // Inizialize.
            videoManifestValidatorTask = new VideoManifestValidatorTask(indexContext.Object, loggerMock.Object, swarmService.Object);
        }

        // Tests.
        [Fact]
        public async Task AppendManifestInVideoWhenIsValidAndHaveAnotherValidManifest()
        {
            // Arrange.
            //first manifest
            var firstMetadata = new VideoManifestMetadataV1(
                "Titletest",
                "Description",
                1234,
                new List<VideoSourceV1>
                {
                    new VideoSourceV1(32, "1080", "Ref1080", null),
                    new VideoSourceV1(32, "720", "Ref720", null)
                },
                null,
                null,
                null,
                null,
                null);
            swarmService
                .Setup(x => x.GetVideoMetadataAsync(manifestHash))
                .ReturnsAsync(firstMetadata);
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            //second manifest for same video
            string secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
            var secondMetadata = new VideoManifestMetadataV1(
                "Titletest",
                "Description2",
                1234,
                new List<VideoSourceV1>
                {
                    new VideoSourceV1(98, "1080", "Ref1080-2", null)
                },
                null,
                null,
                null,
                null,
                null);
            var secondVideoManifest = new VideoManifest(secondManifestHash);
            video.AddManifest(secondVideoManifest);
            var secondIndexContext = new Mock<IIndexDbContext>();
            secondIndexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(secondVideoManifest);
            secondIndexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);
            var secondSwarmService = new Mock<ISwarmService>();
            secondSwarmService
                .Setup(x => x.GetVideoMetadataAsync(secondManifestHash))
                .ReturnsAsync(secondMetadata);
            var secondMetadataVideoValidatorTask = new VideoManifestValidatorTask(secondIndexContext.Object, loggerMock.Object, secondSwarmService.Object);

            // Action.
            await secondMetadataVideoValidatorTask.RunAsync(videoId, secondManifestHash);

            // Assert.
            Assert.True(secondVideoManifest.IsValid);
            Assert.NotNull(secondVideoManifest.ValidationTime);
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == secondManifestHash);
            Assert.Equal(secondManifestHash, video.LastValidManifest!.Manifest.Hash);
        }

        [Fact]
        public async Task FailValidationWithInvalidMetadata()
        {
            // Arrange.
            swarmService
                .Setup(x => x.GetVideoMetadataAsync(manifestHash))
                .ReturnsAsync(
                    () => new VideoManifestMetadataV1(
                        "Titletest",
                        "Description",
                        1234,
                        new List<VideoSourceV1>(),
                        null,
                        null,
                        null,
                        null,
                        null));

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongJson()
        {
            // Arrange.
            swarmService
                .Setup(x => x.GetVideoMetadataAsync(manifestHash))
                .ThrowsAsync(new VideoManifestValidationException(
                    new[] { new ValidationError(ValidationErrorType.JsonConvert, "Unable to parse json") }));

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "Unable to parse json" &&
                    i.ErrorType == ValidationErrorType.JsonConvert);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task InsertManifestInVideoEvenIfIsNotValid()
        {
            // Arrange.
            swarmService
                .Setup(x => x.GetVideoMetadataAsync(manifestHash))
                .ReturnsAsync(() => new VideoManifestMetadataV1(
                    "",
                    "Description",
                    1234,
                    new[] { new VideoSourceV1(null, "720", "ref", null) },
                    null,
                    null,
                    null,
                    null,
                    null));

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task InsertManifestInVideoWhenIsValid()
        {
            // Arrange.
            var metadataVideoDto = new VideoManifestMetadataV1(
                "title",
                "Description",
                1234,
                new[] { new VideoSourceV1(null, "720", "ref", null) },
                null,
                null,
                null,
                null,
                null);
            swarmService
                .Setup(x => x.GetVideoMetadataAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Equal(metadataVideoDto, videoManifest.Metadata);
            Assert.Empty(videoManifest.ValidationErrors);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Equal(manifestHash, video.LastValidManifest!.Manifest.Hash);
        }
    }
}
