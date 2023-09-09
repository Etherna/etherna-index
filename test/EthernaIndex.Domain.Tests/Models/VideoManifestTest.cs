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

using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using System;
using System.Collections.Generic;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoManifestTest
    {
        // Fields.
        readonly string hash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        readonly VideoManifest manifest;

        // Constructors.
        public VideoManifestTest()
        {
            manifest = new VideoManifest(hash);
        }

        [Fact]
        public void Create_Manifest_WithDefaultValue()
        {
            // Assert.
            Assert.Equal(hash, manifest.Manifest.Hash);
            Assert.Null(manifest.IsValid);
            Assert.Null(manifest.ValidationTime);
        }

        [Fact]
        public void FailedValidation_SetValidationFields()
        {
            // Action.
            manifest.FailedValidation(new List<ValidationError> {
                { new ValidationError(ValidationErrorType.Unknown, "Unknown Error") },
                { new ValidationError(ValidationErrorType.InvalidVideoSource, "Invalid Source Video") }
            });

            // Assert.
            Assert.False(manifest.IsValid);
            Assert.Contains(manifest.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.Unknown &&
                    i.ErrorMessage.Equals("Unknown Error", StringComparison.Ordinal));
            Assert.Contains(manifest.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.InvalidVideoSource &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.NotNull(manifest.ValidationTime);
        }

        [Fact]
        public void SuccessfulValidation_SetValidationFields()
        {
            // Action.
            manifest.SucceededValidation(
                new VideoManifestMetadataV2(
                    "TitleTest",
                    "DescTest",
                    12345,
                    new[] { new VideoSourceV2("myPath", "720", 32, "mp4") },
                    null,
                    1,
                    "myBatchId",
                    456,
                    null,
                    null));

            // Assert.
            Assert.True(manifest.IsValid);
            Assert.Empty(manifest.ValidationErrors);
            Assert.NotNull(manifest.ValidationTime);
        }

        [Fact]
        public void SuccessfulValidation_SetMetadata()
        {
            // Arrange.
            var metadata = new VideoManifestMetadataV2(
                "FeddTopicTest",
                "DescTest",
                1,
                new[] { new VideoSourceV2("path1", "10801", 4, "type1") },
                new ThumbnailV2(1.78f, "BlurTst", new[] { new ImageSourceV2(1080, "Test1", "image") }),
                1.78f,
                "myBatchId",
                12345,
                54321,
                "{}");

            // Action.
            manifest.SucceededValidation(metadata);

            // Assert.
            Assert.Equal(metadata, manifest.Metadata);
        }
    }
}
