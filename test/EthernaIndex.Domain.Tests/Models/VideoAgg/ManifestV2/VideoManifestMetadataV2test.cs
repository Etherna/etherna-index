using Etherna.EthernaIndex.Domain.Exceptions;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2
{
    public class VideoManifestMetadataV2test
    {
        // Tests.
        [Fact]
        public void VerifyNotInvalidAspectRatio()
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoManifestMetadataV2(
                    "Title",
                    "Description",
                    1234,
                    new[] { new VideoSourceV2("myPath", "720", 32, "mp4") },
                    null,
                    0,
                    "myBatchId",
                    1234,
                    null,
                    null));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorType == ValidationErrorType.InvalidAspectRatio);
        }
    }
}
