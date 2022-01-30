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

using Etherna.EthernaIndex.Domain.Models.Manifest;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using System;
using System.Collections.Generic;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoTest
    {
        // Fields.
        readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        readonly string encryptKey = "1d111a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f1";
        readonly string hash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        readonly User owner;
        readonly Video video;

        // Constructors.
        public VideoTest()
        {
            owner = new User(address);
            video = new Video(encryptKey, EncryptionType.AES256, hash, owner);
        }

        // Tests.
        [Fact]
        public void Create_Video()
        {
            //Arrange

            //Act

            //Assert
            Assert.Equal(hash, video.ManifestHash.Hash);
            Assert.Equal(encryptKey, video.EncryptionKey);
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(EncryptionType.AES256, video.EncryptionType);
            Assert.Equal(hash, video.ManifestHash.Hash);
            Assert.NotNull(video.Owner);
            Assert.Equal(address, video.Owner.Address);
            Assert.NotNull(video.GetManifest());
        }

        [Fact]
        public void Create_Manifest_WithDefaultValue()
        {
            //Arrange

            //Act
            var manifest = video.GetManifest();


            //Assert
            Assert.Equal(hash, manifest.ManifestHash);
            Assert.Null(manifest.IsValid);
            Assert.Null(manifest.ValidationTime);
        }

        [Fact]
        public void FailedValidation_SetValidationFields()
        {
            //Arrange
            var manifest = video.GetManifest();

            //Act
            manifest.FailedValidation(new List<ErrorDetail> {
                { new ErrorDetail(ValidationErrorType.Generic, "Generic Error") },
                { new ErrorDetail(ValidationErrorType.InvalidVideoSource, "Invalid Source Video") }
            });


            //Assert
            Assert.False(manifest.IsValid);
            Assert.Contains(manifest.ErrorValidationResults,
                i => i.ErrorNumber == ValidationErrorType.Generic &&
                    i.ErrorMessage.Equals("Generic Error", StringComparison.Ordinal));
            Assert.Contains(manifest.ErrorValidationResults,
                i => i.ErrorNumber == ValidationErrorType.InvalidVideoSource &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.NotNull(manifest.ValidationTime);
        }

        [Fact]
        public void SuccessfulValidation_SetValidationFields()
        {
            //Arrange
            var manifest = video.GetManifest();

            //Act
            manifest.SuccessfulValidation(
                "FeddTopicTest",
                "TitleTest",
                "DescTest",
                "OriginalTest",
                1,
                new SwarmImageRaw(
                    1,
                    "BlurTst",
                    new Dictionary<string, string> { { "1080", "Test1" }, { "720", "Test2" } })
                );


            //Assert
            Assert.True(manifest.IsValid);
            Assert.Empty(manifest.ErrorValidationResults);
            Assert.NotNull(manifest.ValidationTime);
        }

        [Fact]
        public void SuccessfulValidation_SetMetadataFields()
        {
            //Arrange
            var manifest = video.GetManifest();
            var feed = "FeedTest";
            var title = "FeddTopicTest";
            var desc = "DescTest";
            var original = "OriginalTest";
            var duration = 1;
            var blur = "BlurTst";
            var aspectRatio = 1;
            var source = new Dictionary<string, string> { { "1080", "Test1" }, { "720", "Test2" } };

            //Act
            manifest.SuccessfulValidation(
                feed,
                title,
                desc,
                original,
                duration, 
                new SwarmImageRaw(aspectRatio, blur, source));


            //Assert
            Assert.Equal(feed, manifest.FeedTopicId);
            Assert.Equal(title, manifest.Title);
            Assert.Equal(desc, manifest.Description);
            Assert.Equal(duration, manifest.Duration);
            Assert.Equal(original, manifest.OriginalQuality);
            Assert.NotNull(manifest.Thumbnail);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.Equal(blur, manifest.Thumbnail.BlurHash);
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
            var manifest = video.GetManifest();
            var feed = "FeedTest";
            var title = "FeddTopicTest";
            var desc = "DescTest";
            var original = "OriginalTest";
            var duration = 1;

            //Act
            manifest.SuccessfulValidation(
                feed,
                title,
                desc,
                original,
                duration,
                null);


            //Assert
            Assert.Equal(feed, manifest.FeedTopicId);
            Assert.Equal(title, manifest.Title);
            Assert.Equal(desc, manifest.Description);
            Assert.Equal(duration, manifest.Duration);
            Assert.Equal(original, manifest.OriginalQuality);
            Assert.Null(manifest.Thumbnail);
        }
    }
}
