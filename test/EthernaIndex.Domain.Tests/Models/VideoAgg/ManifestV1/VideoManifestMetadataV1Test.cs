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

using Etherna.EthernaIndex.Domain.Exceptions;
using System.Collections.Generic;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1
{
    public class VideoManifestMetadataV1Test
    {
        // Tests.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void VerifyNotEmptySources(bool sourcesIsNull)
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoManifestMetadataV1(
                    "Titletest",
                    "Description",
                    1234,
                    sourcesIsNull ? null! : new List<VideoSourceV1>(),
                    null,
                    null,
                    null,
                    null,
                    null));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorMessage == "Missing sources" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
        }

        [Fact]
        public void VerifyNotNullDescription()
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoManifestMetadataV1(
                    "Titletest",
                    null!,
                    1234,
                    new[] { new VideoSourceV1(null, "720", "ref", null) },
                    null,
                    null,
                    null,
                    null,
                    null));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.MissingDescription);
        }

        [Fact]
        public void VerifyNotWrongTitle()
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoManifestMetadataV1(
                    "",
                    "Description",
                    1234,
                    new[] { new VideoSourceV1(null, "720", "ref", null) },
                    null,
                    null,
                    null,
                    null,
                    null));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.MissingTitle);
        }
    }
}
