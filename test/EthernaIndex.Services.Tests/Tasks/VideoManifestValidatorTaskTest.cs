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

using Etherna.BeeNet;
using Etherna.BeeNet.Models;
using Etherna.BeeNet.Stores;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Infrastructure;
using Etherna.Sdk.Tools.Video.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Nethereum.Util;
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
        private readonly Mock<ISwarmClient> beeClientMock = new();
        private readonly VideoManifestValidatorTask videoManifestValidatorTask;
        private readonly SwarmHash manifestHash = "1a345a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
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
            video = new Video(owner);
            videoManifest = new VideoManifest(manifestHash);
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
                manifestHash,
                new(1,
                    null,
                    DateTimeOffset.Now,
                    "Description",
                    TimeSpan.FromSeconds(600), 
                    "Title",
                    AddressUtil.ZERO_ADDRESS,
                    null,
                    [
                        new VideoManifestVideoSource("1080.mp4", VideoType.Mp4, "1080p", 32, [], SwarmHash.Zero),
                        new VideoManifestVideoSource("720.mp4", VideoType.Mp4, "720p", 32, [], SwarmHash.Zero)
                    ],
                    new VideoManifestImage(1, "", [new VideoManifestImageSource("thumb.jpg", ImageType.Jpeg, 100, SwarmHash.Zero)]),
                    []),
                []);
            swarmServiceMock
                .Setup(x => x.GetPublishedVideoManifestAsync(manifestHash, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(firstManifest);
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash.ToString());

            //second manifest for same video
            SwarmHash secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
            var secondManifest = new PublishedVideoManifest(
                manifestHash,
                new(1,
                    null,
                    DateTimeOffset.Now,
                    "Description2",
                    TimeSpan.FromSeconds(600), 
                    "Title2",
                    AddressUtil.ZERO_ADDRESS,
                    null,
                    [
                        new VideoManifestVideoSource("1080.mp4", VideoType.Mp4, "1080p", 98, [], SwarmHash.Zero)
                    ],
                    new VideoManifestImage(1, "", []),
                    []),
                []);
            var secondVideoManifest = new VideoManifest(secondManifestHash);
            
            video.AddManifest(secondVideoManifest);
            var secondIndexContext = new Mock<IIndexDbContext>();
            secondIndexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(secondVideoManifest);
            secondIndexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);
            
            var secondSwarmService = new Mock<ISwarmService>();
            secondSwarmService
                .Setup(x => x.GetPublishedVideoManifestAsync(secondManifestHash, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(secondManifest);
            var secondMetadataVideoValidatorTask = new VideoManifestValidatorTask(
                beeClientMock.Object,
                secondIndexContext.Object,
                loggerMock.Object,
                secondSwarmService.Object);

            // Action.
            await secondMetadataVideoValidatorTask.RunAsync(videoId, secondManifestHash.ToString());

            // Assert.
            Assert.True(secondVideoManifest.IsValid);
            Assert.NotNull(secondVideoManifest.ValidationTime);
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash == manifestHash);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash == secondManifestHash);
            Assert.Equal(secondManifestHash, video.LastValidManifest!.ManifestHash);
        }

        [Fact]
        public async Task FailValidationWithInvalidMetadata()
        {
            // Arrange.
            swarmServiceMock.Setup(x => x.GetPublishedVideoManifestAsync(manifestHash, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(new PublishedVideoManifest(manifestHash, null, [new ValidationError(ValidationErrorType.Unknown)]));
        
            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash.ToString());
        
            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests, i => i.ManifestHash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongJson()
        {
            // Arrange.
            swarmServiceMock.Setup(x => x.GetPublishedVideoManifestAsync(manifestHash, It.IsAny<IReadOnlyChunkStore>()))
                .Returns(Task.FromResult(new PublishedVideoManifest(
                    manifestHash, null, [new(ValidationErrorType.JsonConvert, "Unable to parse json")])));

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash.ToString());

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "Unable to parse json" &&
                    i.ErrorType == ValidationErrorType.JsonConvert);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task InsertManifestInVideoWhenIsValid()
        {
            // Arrange.
            var publishedVideoManifest = new PublishedVideoManifest(
                manifestHash,
                new Sdk.Tools.Video.Models.VideoManifest(
                    1,
                    null,
                    DateTimeOffset.Now,
                    "Description",
                    TimeSpan.FromSeconds(600), 
                    "Title",
                    AddressUtil.ZERO_ADDRESS,
                    null,
                    [
                        new VideoManifestVideoSource("720.mp4", VideoType.Mp4, "720p", 32, [], SwarmHash.Zero)
                    ],
                    new VideoManifestImage(1, "", []),
                    []),
                []);
            swarmServiceMock
                .Setup(x => x.GetPublishedVideoManifestAsync(manifestHash, It.IsAny<IReadOnlyChunkStore>()))
                .ReturnsAsync(publishedVideoManifest);
        
            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash.ToString());
        
            // Assert.
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Equal(publishedVideoManifest.Manifest!.Title, (videoManifest.Metadata as VideoManifestMetadataV2)!.Title);
            Assert.Empty(videoManifest.ValidationErrors);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash == manifestHash);
            Assert.Equal(manifestHash, video.LastValidManifest!.ManifestHash);
        }
    }
}
