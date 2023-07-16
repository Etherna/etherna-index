using Etherna.EthernaIndex.Domain.Exceptions;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2
{
    public class VideoSourceV2Test
    {
        [Fact]
        public void VerifyNotWrongPathSources()
        {
            // Action.
            var exception = Assert.Throws<VideoManifestValidationException>(
                () => new VideoSourceV2("", "1080", 32, "mp4"));

            // Assert.
            Assert.IsType<VideoManifestValidationException>(exception);
            Assert.Contains(exception.ValidationErrors,
                i => i.ErrorMessage == "Video source has empty path" &&
                    i.ErrorType == ValidationErrorType.InvalidVideoSource);
        }
    }
}
