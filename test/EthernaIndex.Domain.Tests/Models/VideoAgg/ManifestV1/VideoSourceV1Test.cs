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
