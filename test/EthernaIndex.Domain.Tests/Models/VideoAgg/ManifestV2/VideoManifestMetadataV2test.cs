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
