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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Infrastructure;
using Etherna.Sdk.Tools.Video.Models;
using Etherna.SwarmSdk;
using Etherna.SwarmSdk.Models;
using Etherna.SwarmSdk.Stores;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using VideoManifest = Etherna.EthernaIndex.Domain.Models.VideoAgg.VideoManifest;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public class VideoManifestValidatorTaskTest
    {
        // Fields.
        private readonly PostageBatchId batchId = "db7fde96b8eb94c3ec43cf6547cf045b2a719a3d8b27489e08bb33c32afece4e";
        private readonly Mock<ISwarmClient> beeClientMock = new();
        private readonly VideoManifestValidatorTask videoManifestValidatorTask;
        private readonly SwarmReference manifestReference = "1a345a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        private readonly string videoId = "videoId";
        private readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        private readonly Mock<UserSharedInfo> userSharedInfoMock = new();
        private readonly Video video;
        private readonly VideoManifest videoManifest;
        private readonly Mock<IIndexDbContext> indexContext;
        private readonly Mock<ILogger<VideoManifestValidatorTask>> loggerMock;
        private readonly Mock<ISwarmService> swarmServiceMock;

        // Constructor.
        public VideoManifestValidatorTaskTest()
        {
            userSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            var owner = new User(userSharedInfoMock.Object);
            video = new Video(owner, batchId);
            videoManifest = new VideoManifest(manifestReference);
            video.AddManifest(videoManifest);

            loggerMock = new Mock<ILogger<VideoManifestValidatorTask>>();
            swarmServiceMock = new Mock<ISwarmService>();

            // Mock Db Data.
            indexContext = new Mock<IIndexDbContext>();
            indexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(videoManifest);
            indexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);

            // Inizialize.
            videoManifestValidatorTask = new VideoManifestValidatorTask(
                beeClientMock.Object,
                indexContext.Object,
                loggerMock.Object,
                swarmServiceMock.Object);
        }

        // Tests.
        [Fact]
        public async Task AppendManifestInVideoWhenIsValidAndHaveAnotherValidManifest()
        {
            // Arrange.
            //first manifest
            var firstManifest = new PublishedVideoManifest(
                manifestReference,
                new(1,
                    DateTimeOffset.Now,
                    "Description",
                    TimeSpan.FromSeconds(600), 
                    "Title",
                    EthAddress.Zero,
                    null,
                    [
                        new VideoManifestVideoSource("1080.mp4", VideoType.Mp4, "1080p", 32, [], SwarmReference.PlainZero),
                        new VideoManifestVideoSource("720.mp4", VideoType.Mp4, "720p", 32, [], SwarmReference.PlainZero)
                    ],
                    new VideoManifestImage(1, "", [new VideoManifestImageSource("thumb.jpg", ImageType.Jpeg, 100, SwarmReference.PlainZero)]),
                    []),
                []);
            swarmServiceMock
                .Setup(x => x.GetPublishedVideoManifestAsync(manifestReference, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(firstManifest);
            await videoManifestValidatorTask.RunAsync(videoId, manifestReference.ToString());

            //second manifest for same video
            SwarmReference secondManifestReference = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
            var secondManifest = new PublishedVideoManifest(
                manifestReference,
                new(1,
                    DateTimeOffset.Now,
                    "Description2",
                    TimeSpan.FromSeconds(600), 
                    "Title2",
                    EthAddress.Zero,
                    null,
                    [
                        new VideoManifestVideoSource("1080.mp4", VideoType.Mp4, "1080p", 98, [], SwarmReference.PlainZero)
                    ],
                    new VideoManifestImage(1, "", []),
                    []),
                []);
            var secondVideoManifest = new VideoManifest(secondManifestReference);
            
            video.AddManifest(secondVideoManifest);
            var secondIndexContext = new Mock<IIndexDbContext>();
            secondIndexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(secondVideoManifest);
            secondIndexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);
            
            var secondSwarmService = new Mock<ISwarmService>();
            secondSwarmService
                .Setup(x => x.GetPublishedVideoManifestAsync(secondManifestReference, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(secondManifest);
            var secondMetadataVideoValidatorTask = new VideoManifestValidatorTask(
                beeClientMock.Object,
                secondIndexContext.Object,
                loggerMock.Object,
                secondSwarmService.Object);

            // Action.
            await secondMetadataVideoValidatorTask.RunAsync(videoId, secondManifestReference.ToString());

            // Assert.
            Assert.True(secondVideoManifest.IsValid);
            Assert.NotNull(secondVideoManifest.ValidationTime);
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == manifestReference);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == secondManifestReference);
            Assert.Equal(secondManifestReference, video.LastValidManifest!.ManifestReference);
        }

        [Fact]
        public async Task FailValidationWithInvalidMetadata()
        {
            // Arrange.
            swarmServiceMock.Setup(x => x.GetPublishedVideoManifestAsync(manifestReference, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(new PublishedVideoManifest(manifestReference, null, [new ValidationError(ValidationErrorType.Unknown)]));
        
            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestReference.ToString());
        
            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests, i => i.ManifestReference == manifestReference);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithUnsupportedManifestVersion()
        {
            // Arrange.
            var publishedVideoManifest = new PublishedVideoManifest(
                manifestReference,
                new Sdk.Tools.Video.Models.VideoManifest(
                    1,
                    DateTimeOffset.Now,
                    "Description",
                    TimeSpan.FromSeconds(600),
                    "Title",
                    EthAddress.Zero,
                    null,
                    [
                        new VideoManifestVideoSource("720p.mp4", VideoType.Mp4, "720p", 32, [], SwarmReference.PlainZero)
                    ],
                    new VideoManifestImage(1, "", []),
                    []),
                [],
                new Version(1, 2));
            swarmServiceMock
                .Setup(x => x.GetPublishedVideoManifestAsync(manifestReference, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(publishedVideoManifest);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestReference.ToString());

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.UnsupportedManifestVersion);
            Assert.Null(videoManifest.Metadata);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == manifestReference);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongJson()
        {
            // Arrange.
            swarmServiceMock.Setup(x => x.GetPublishedVideoManifestAsync(manifestReference, It.IsAny<IReadOnlyChunkStore>()))
                .Returns(Task.FromResult(new PublishedVideoManifest(
                    manifestReference, null, [new(ValidationErrorType.JsonConvert, "Unable to parse json")])));

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestReference.ToString());

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "Unable to parse json" &&
                    i.ErrorType == ValidationErrorType.JsonConvert);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == manifestReference);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task InsertManifestInVideoWhenIsValid()
        {
            // Arrange.
            var publishedVideoManifest = new PublishedVideoManifest(
                manifestReference,
                new Sdk.Tools.Video.Models.VideoManifest(
                    1,
                    DateTimeOffset.Now,
                    "Description",
                    TimeSpan.FromSeconds(600), 
                    "Title",
                    EthAddress.Zero,
                    null,
                    [
                        new VideoManifestVideoSource("720.mp4", VideoType.Mp4, "720p", 32, [], SwarmReference.PlainZero)
                    ],
                    new VideoManifestImage(1, "", []),
                    []),
                [],
                new Version(2, 1));
            swarmServiceMock
                .Setup(x => x.GetPublishedVideoManifestAsync(manifestReference, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(publishedVideoManifest);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestReference.ToString());

            // Assert.
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Equal(publishedVideoManifest.Manifest!.Title, (videoManifest.Metadata as VideoManifestMetadataV2)!.Title);
            Assert.Empty(videoManifest.ValidationErrors);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestReference == manifestReference);
            Assert.Equal(manifestReference, video.LastValidManifest!.ManifestReference);
        }
    }
}
