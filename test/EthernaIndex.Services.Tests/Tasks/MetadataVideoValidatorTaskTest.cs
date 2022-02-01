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
using Etherna.EthernaIndex.Domain.Models.Manifest;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.EthernaIndex.Swarm;
using Etherna.EthernaIndex.Swarm.DtoModel;
using Etherna.MongODM.Core.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EthernaIndex.Services.Tests.Tasks
{
    public class MetadataVideoValidatorTaskTest
    {
        // Fields.
        readonly MetadataVideoValidatorTask metadataVideoValidatorTask;
        readonly string manifestHash = "manifestHash";
        readonly string videoId = "videoId";
        readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        readonly string encryptKey = "1d111a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f1";
        readonly string hash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        readonly Video video;
        readonly VideoManifest videoManifest;
        readonly Mock<IIndexContext> indexContext;
        readonly Mock<ISwarmService> swarmService;

        // Constructors.
        public MetadataVideoValidatorTaskTest()
        {
            var owner = new User(address);
            video = new Video(encryptKey, EncryptionType.AES256, hash, owner);
            videoManifest = new VideoManifest(manifestHash);

            swarmService = new Mock<ISwarmService>();

            // Mock Db Data.
            indexContext = new Mock<IIndexContext>();
            indexContext.Setup(_ => _.VideoManifest.FindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(videoManifest);
            indexContext.Setup(_ => _.Videos.FindOneAsync(It.IsAny<Expression<Func<Video, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(video);

            // Inizialize.
            metadataVideoValidatorTask = new MetadataVideoValidatorTask(indexContext.Object, swarmService.Object);
        }

        // Tests.
        [Fact]
        public async Task ShouldValidateManifest_WithCorrectData()
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
            Assert.Equal(metadataVideoDto.Id, videoManifest.FeedTopicId);
            Assert.Equal(metadataVideoDto.Description, videoManifest.Description);
            Assert.Equal(metadataVideoDto.Duration, videoManifest.Duration);
            Assert.Equal(metadataVideoDto.OriginalQuality, videoManifest.OriginalQuality);
            Assert.Equal(metadataVideoDto.Title, videoManifest.Title);
            Assert.Empty(videoManifest.ErrorValidationResults);
        }

        [Fact]
        public async Task ShouldInvalidateManifest_WithWrongTitle()
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
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "MissingTitle" &&
                    i.ErrorNumber == ValidationErrorType.MissingTitle);
        }

        [Fact]
        public async Task ShouldInvalidateManifest_WithWrongJson()
        {
            //Arrange
            swarmService
                .Setup(x => x.GetMetadataVideoAsync(manifestHash))
                .ThrowsAsync(new MetadataVideoException("Unable to cast json"));

            //Act
            await metadataVideoValidatorTask.RunAsync(videoId, manifestHash);

            //Assert
            Assert.False(videoManifest.IsValid);
            Assert.Contains(videoManifest.ErrorValidationResults,
                i => i.ErrorMessage == "Unable to cast json" &&
                    i.ErrorNumber == ValidationErrorType.JsonConvert);
        }
    }
}
