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
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.Sdk.Tools.Video.Models;
using System;
using Xunit;
using VideoManifest = Etherna.EthernaIndex.Domain.Models.VideoAgg.VideoManifest;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoManifestTest
    {
        // Fields.
        readonly SwarmHash hash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
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
            Assert.Equal(hash, manifest.ManifestHash);
            Assert.Null(manifest.IsValid);
            Assert.Null(manifest.ValidationTime);
        }

        [Fact]
        public void FailedValidation_SetValidationFields()
        {
            // Action.
            manifest.FailedValidation(
            [
                new(ValidationErrorType.Unknown, "Unknown Error"),
                new(ValidationErrorType.InvalidVideoSource, "Invalid Source Video")
            ]);

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
                    [new VideoSourceV2("myPath", "720", 32, "mp4")],
                    null,
                    1,
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
                [new VideoSourceV2("path1", "10801", 4, "type1")],
                new ThumbnailV2(1.78f, "BlurTst", new[] { new ImageSourceV2(1080, "Test1", "image") }),
                1.78f,
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
