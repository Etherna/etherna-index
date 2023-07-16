using Etherna.EthernaIndex.Domain.Exceptions;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1
{
    public class VideoSourceV1Test
    {
        // Tests.
        [Fact]
        public void VerifyNotWrongQualitySources()
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoSourceV1(32, "", "Ref1080", null));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorMessage == "Video source has empty quality" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
        }

        [Fact]
        public void VerifyNotWrongReferenceSources()
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoSourceV1(32, "1080", "", null));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorMessage == "Video source has empty reference" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
        }
    }
}
