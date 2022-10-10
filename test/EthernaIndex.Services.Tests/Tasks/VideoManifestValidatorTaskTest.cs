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
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.EthernaIndex.Swarm;
using Etherna.EthernaIndex.Swarm.Exceptions;
using Etherna.EthernaIndex.Swarm.Models;
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
            var firstMetadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "1080", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "Ref720", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(firstMetadataVideoDto);
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);
            //second manifest for same video
            string secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
            var secondMetadataVideoDto = new MetadataVideo(
                null,
                "Description2",
                20,
                1234,
                "456",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(2, "10802", "Ref10802", 98)
                },
                null,
                "Titletest",
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
                .Setup(x => x.GetMetadataVideoAsync(secondManifestHash))
                .ReturnsAsync(secondMetadataVideoDto);
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
        public async Task FailValidationWithEmptySources()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>(),
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "Missing sources" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithNullDescription()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                null!,
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "1080", "Ref1080", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.MissingDescription);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithNullSources()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                null!,
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "Missing sources" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongJson()
        {
            // Arrange.
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ThrowsAsync(new MetadataVideoException("Unable to cast json"));

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "Unable to cast json" &&
                    i.ErrorType == ValidationErrorType.JsonConvert);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongQualitySources()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "Ref720", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "empty quality" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongReferenceSources()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "1080", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "[720] empty reference" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task FailValidationWithWrongTitle()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "1080", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "Ref720", 32)
                },
                null,
                null!,
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ValidationErrors,
                i => i.ErrorMessage == "MissingTitle" &&
                    i.ErrorType == ValidationErrorType.MissingTitle);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Null(video.LastValidManifest);
        }

        [Fact]
        public async Task InsertManifestInVideoEvenIfIsNotValid()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "Ref720", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

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
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                address,
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "1080", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "Ref720", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests,
                i => i.Manifest.Hash == manifestHash);
            Assert.Equal(manifestHash, video.LastValidManifest!.Manifest.Hash);
        }

        [Fact]
        public async Task SucceedValidationWithCorrectData()
        {
            // Arrange.
            var metadataVideoDto = new MetadataVideo(
                null,
                "Description",
                10,
                1234,
                "123",
                "",
                new List<MetadataVideoSource>
                {
                    new MetadataVideoSource(1, "1080", "Ref1080", 32),
                    new MetadataVideoSource(2, "720", "Ref720", 32)
                },
                null,
                "Titletest",
                null);
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            // Action.
            await videoManifestValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Equal(metadataVideoDto.Description, videoManifest.Description);
            Assert.Equal(metadataVideoDto.Duration, videoManifest.Duration);
            Assert.Equal(metadataVideoDto.OriginalQuality, videoManifest.OriginalQuality);
            Assert.Equal(metadataVideoDto.Title, videoManifest.Title);
            Assert.Empty(videoManifest.ValidationErrors);
            Assert.Contains(videoManifest.Sources,
                i => i.Bitrate == 1 &&
                    i.Quality == "1080" &&
                    i.Reference == "Ref1080" &&
                    i.Size == 32);
            Assert.Contains(videoManifest.Sources,
                i => i.Bitrate == 2 &&
                    i.Quality == "720" &&
                    i.Reference == "Ref720" &&
                    i.Size == 32);
            Assert.Equal(manifestHash, video.LastValidManifest!.Manifest.Hash);
        }
    }
}
