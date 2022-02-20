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
using Etherna.EthernaIndex.Domain.Models.ManifestAgg;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.EthernaIndex.Swarm;
using Etherna.EthernaIndex.Swarm.DtoModel;
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
    public class MetadataVideoValidatorTaskTest
    {
        // Fields.
        private readonly MetadataVideoValidatorTask metadataVideoValidatorTask;
        private readonly string manifestHash = "1a345a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        private readonly string videoId = "videoId";
        private readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        private readonly string encryptKey = "1d111a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f1";
        private readonly Mock<UserSharedInfo> userSharedInfoMock = new();
        private readonly Video video;
        private readonly VideoManifest videoManifest;
        private readonly Mock<IIndexDbContext> indexContext;
        private readonly Mock<ISwarmService> swarmService;

        // Constructors.
        public MetadataVideoValidatorTaskTest()
        {
            userSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            var owner = new User(userSharedInfoMock.Object);
            video = new Video(encryptKey, EncryptionType.AES256, owner);
            videoManifest = new VideoManifest(manifestHash, video);

            swarmService = new Mock<ISwarmService>();

            // Mock Db Data.
            indexContext = new Mock<IIndexDbContext>();
            indexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(videoManifest);
            indexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);

            // Inizialize.
            metadataVideoValidatorTask = new MetadataVideoValidatorTask(indexContext.Object, swarmService.Object);
        }

        // Tests.
        [Fact]
        public async Task ValidateManifest_True_WithCorrectData()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                },
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Equal(metadataVideoDto.Id, videoManifest.FeedTopicId);
            Assert.Equal(metadataVideoDto.Description, videoManifest.Description);
            Assert.Equal(metadataVideoDto.Duration, videoManifest.Duration);
            Assert.Equal(metadataVideoDto.OriginalQuality, videoManifest.OriginalQuality);
            Assert.Equal(metadataVideoDto.Title, videoManifest.Title);
            Assert.Empty(videoManifest.ErrorValidationResults);
            Assert.Contains(videoManifest.Sources,
                i => i.Bitrate == 1 &&
                    i.Quality == "1080" &&
                    i.Reference == "Ref1080" &&
                    i.Size == 32);
            Assert.Contains(videoManifest.Sources,
                i => i.Bitrate == null &&
                    i.Quality == "720" &&
                    i.Reference == "Ref720" &&
                    i.Size == 32);
            Assert.Contains(videoManifest.Sources,
                i => i.Bitrate == 1 &&
                    i.Quality == "360" &&
                    i.Reference == "Ref360" &&
                    i.Size == null);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(manifestHash, video.GetLastValidManifest().ManifestHash.Hash);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Fact]
        public async Task ValidateManifest_False_WithWrongTitle()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                }
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "MissingTitle" &&
                    i.ErrorNumber == ValidationErrorType.MissingTitle);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_False_WithWrongJson()
        {
            //Arrange
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ThrowsAsync(new MetadataVideoException("Unable to cast json"));

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "Unable to cast json" &&
                    i.ErrorNumber == ValidationErrorType.JsonConvert);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_False_WithEmptySources()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>(),
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "Missing sources" &&
                    i.ErrorNumber == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_False_WithNullSources()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>(),
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "Missing sources" &&
                    i.ErrorNumber == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_False_WithWrongReferenceSources()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                },
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "[720] empty reference" &&
                    i.ErrorNumber == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_False_WithWrongQualitySources()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                },
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "empty quality" &&
                    i.ErrorNumber == ValidationErrorType.InvalidVideoSource);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_InsertManifestInVideo_WhenIsValid()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                },
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.True(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(manifestHash, video.GetLastValidManifest().ManifestHash.Hash);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Fact]
        public async Task ValidateManifest_InsertManifestInVideo_EvenIsNotValid()
        {
            //Arrange
            var metadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                }
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(metadataVideoDto);

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            // Assert.
            Assert.False(videoManifest.IsValid);
            Assert.NotNull(videoManifest.ValidationTime);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Null(video.GetLastValidManifest());
        }

        [Fact]
        public async Task ValidateManifest_AppendManifestInVideo_WhenIsValidAndHaveAnotherValidManifest()
        {
            // Arrange.
            var firstMetadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                },
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(firstMetadataVideoDto);
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);
            //second manifest for same video
            string secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
            var secondMetadataVideoDto = new MetadataVideoDto
            {
                Description = "Description2",
                Duration = 20,
                Id = "FeddId2",
                OriginalQuality = "456",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 2, Quality = "10802", Reference = "Ref10802", Size = 98 },
                },
                Title = "Titletest2"
            };
            var secondVideoManifest = new VideoManifest(secondManifestHash, video);
            var secondIndexContext = new Mock<IIndexDbContext>();
            secondIndexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(secondVideoManifest);
            secondIndexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);
            var secondSwarmService = new Mock<ISwarmService>();
            secondSwarmService
                .Setup(x => x.GetMetadataVideoAsync(secondManifestHash))
                .ReturnsAsync(secondMetadataVideoDto);
            var secondMetadataVideoValidatorTask = new MetadataVideoValidatorTask(secondIndexContext.Object, secondSwarmService.Object);

            //Act
            await secondMetadataVideoValidatorTask.RunAsync(videoId, secondManifestHash);

            //Assert
            Assert.True(secondVideoManifest.IsValid);
            Assert.NotNull(secondVideoManifest.ValidationTime);
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == secondManifestHash);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(secondManifestHash, video.GetLastValidManifest().ManifestHash.Hash);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        [Fact]
        public async Task ValidateManifest_AppendManifestInVideo_EvenNotValidAndHaveAnotherValidManifest()
        {
            // Arrange.
            var firstMetadataVideoDto = new MetadataVideoDto
            {
                Description = "Description",
                Duration = 10,
                Id = "FeddId",
                OriginalQuality = "123",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "1080", Reference = "Ref1080", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = null, Quality = "720", Reference = "Ref720", Size = 32 },
                    new MetadataVideoSourceDto { Bitrate = 1, Quality = "360", Reference = "Ref360", Size = null }
                },
                Title = "Titletest"
            };
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ReturnsAsync(firstMetadataVideoDto);
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);
            //second manifest for same video
            string secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
            var secondMetadataVideoDto = new MetadataVideoDto
            {
                Description = "Description2",
                Duration = 20,
                Id = "FeddId2",
                OriginalQuality = "456",
                Sources = new List<MetadataVideoSourceDto>
                {
                    new MetadataVideoSourceDto { Bitrate = 2, Quality = "10802", Reference = "Ref10802", Size = 98 },
                }
            };
            var secondVideoManifest = new VideoManifest(secondManifestHash, video);
            var secondIndexContext = new Mock<IIndexDbContext>();
            secondIndexContext.Setup(_ => _.VideoManifests.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(secondVideoManifest);
            secondIndexContext.Setup(_ => _.Videos.FindOneAsync(videoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);
            var secondSwarmService = new Mock<ISwarmService>();
            secondSwarmService
                .Setup(x => x.GetMetadataVideoAsync(secondManifestHash))
                .ReturnsAsync(secondMetadataVideoDto);
            var secondMetadataVideoValidatorTask = new MetadataVideoValidatorTask(secondIndexContext.Object, secondSwarmService.Object);

            //Act
            await secondMetadataVideoValidatorTask.RunAsync(videoId, secondManifestHash);

            //Assert
            Assert.False(secondVideoManifest.IsValid);
            Assert.NotNull(secondVideoManifest.ValidationTime);
            Assert.Equal(2, video.VideoManifests.Count());
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == manifestHash);
            Assert.Contains(video.VideoManifests,
                i => i.ManifestHash.Hash == secondManifestHash);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(manifestHash, video.GetLastValidManifest().ManifestHash.Hash);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }


    }
}
