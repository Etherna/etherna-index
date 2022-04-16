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
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoManifestTest
    {
        // Fields.
        readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        readonly string hash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        readonly VideoManifest manifest;
        private readonly Mock<UserSharedInfo> userSharedInfoMock = new();

        // Constructors.
        public VideoManifestTest()
        {
            userSharedInfoMock.Setup(s => s.EtherAddress).Returns(address);
            var user = new User(userSharedInfoMock.Object);
            manifest = new VideoManifest(hash);
        }

        [Fact]
        public void Create_Manifest_WithDefaultValue()
        {
            //Assert
            Assert.Equal(hash, manifest.Manifest.Hash);
            Assert.Null(manifest.IsValid);
            Assert.Null(manifest.ValidationTime);
        }

        [Fact]
        public void FailedValidation_SetValidationFields()
        {
            //Act
            manifest.FailedValidation(new List<ErrorDetail> {
                { new ErrorDetail(ValidationErrorType.Unknown, "Unknown Error") },
                { new ErrorDetail(ValidationErrorType.InvalidVideoSource, "Invalid Source Video") }
            });


            //Assert
            Assert.False(manifest.IsValid);
            Assert.Contains(manifest.ErrorValidationResults,
                i => i.ErrorType == ValidationErrorType.Unknown &&
                    i.ErrorMessage.Equals("Unknown Error", StringComparison.Ordinal));
            Assert.Contains(manifest.ErrorValidationResults,
                i => i.ErrorType == ValidationErrorType.InvalidVideoSource &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.NotNull(manifest.ValidationTime);
        }

        [Fact]
        public void SuccessfulValidation_SetValidationFields()
        {
            //Act
            manifest.SuccessfulValidation(
                "DescTest",
                1,
                "OriginalTest",
                "TitleTest",
                new SwarmImageRaw(
                    1,
                    "BlurTst",
                    new Dictionary<string, string> { { "1080", "Test1" }, { "720", "Test2" } }),
                new List<VideoSource>());


            //Assert
            Assert.True(manifest.IsValid);
            Assert.Empty(manifest.ErrorValidationResults);
            Assert.NotNull(manifest.ValidationTime);
        }

        [Fact]
        public void SuccessfulValidation_SetMetadataFields()
        {
            //Arrange
            var title = "FeddTopicTest";
            var desc = "DescTest";
            var original = "OriginalTest";
            var duration = 1;
            var videoSources = new List<VideoSource> { new VideoSource(1, "10801", "reff1", 4), new VideoSource(null, "321", "reff2", 100) };
            var blur = "BlurTst";
            var aspectRatio = 1;
            var source = new Dictionary<string, string> { { "1080", "Test1" }, { "720", "Test2" } };

            //Act
            manifest.SuccessfulValidation(
                desc,
                duration,
                original,
                title,
                new SwarmImageRaw(aspectRatio, blur, source),
                videoSources);


            //Assert
            Assert.Equal(title, manifest.Title);
            Assert.Equal(desc, manifest.Description);
            Assert.Equal(duration, manifest.Duration);
            Assert.Equal(original, manifest.OriginalQuality);
            Assert.Contains(manifest.Sources,
               i => i.Bitrate == 1 &&
                   i.Quality == "10801" &&
                   i.Reference == "reff1" &&
                   i.Size == 4);
            Assert.Contains(manifest.Sources,
               i => i.Bitrate == null &&
                   i.Quality == "321" &&
                   i.Reference == "reff2" &&
                   i.Size == 100);
            Assert.NotNull(manifest.Thumbnail);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(blur, manifest.Thumbnail.Blurhash);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.Equal(aspectRatio, manifest.Thumbnail.AspectRatio);
            Assert.NotNull(manifest.Thumbnail.Sources);
            Assert.Contains(manifest.Thumbnail.Sources,
                i => i.Key == "1080" &&
                    i.Value == "Test1");
            Assert.Contains(manifest.Thumbnail.Sources,
                i => i.Key == "720" &&
                    i.Value == "Test2");
        }

        [Fact]
        public void SuccessfulValidation_SetNullSwarmImageRaw()
        {
            //Arrange
            var title = "FeddTopicTest";
            var desc = "DescTest";
            var original = "OriginalTest";
            var duration = 1;
            var videoSources = new List<VideoSource> { new VideoSource(1, "10801", "reff1", 4), new VideoSource(null, "321", "reff2", 100) };

            //Act
            manifest.SuccessfulValidation(
                desc,
                duration,
                original,
                title,
                null,
                videoSources);

            //Assert
            Assert.Null(manifest.Thumbnail);
        }
    }
}
