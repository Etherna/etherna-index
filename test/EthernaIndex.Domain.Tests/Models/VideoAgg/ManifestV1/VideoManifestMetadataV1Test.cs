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
