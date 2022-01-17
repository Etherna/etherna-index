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

using System;
using System.Collections.Generic;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{

    public class ValidationResultTest
    {
        User owner;
        VideoValidationResult inizializedVideoValidationResult;
        string hash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";

        public ValidationResultTest()
        {
            owner = new User("0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0");
            inizializedVideoValidationResult = new VideoValidationResult(hash, owner);
            inizializedVideoValidationResult.InizializeManifest("", "", "", "", 0, null, null);
        }


        [Fact]
        public void Create_ValidationResult_WithDefaultValue()
        {
            //Arrange

            //Act
            var validationResult = new VideoValidationResult(hash, owner);


            //Assert
            Assert.Equal(hash, validationResult.ManifestHash.Hash);
            Assert.Null(validationResult.IsValid);
            Assert.Null(validationResult.LastCheck);
        }

        [Fact]
        public void SetResult_ThrowException_WhenNotInizializedManifest()
        {
            //Arrange


            //Act Assert
            var validationResult = new VideoValidationResult(hash, owner);
            Assert.Throws<InvalidOperationException>(() => validationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string>()));
        }

        [Fact]
        public void SetResult_InsertError_WhenHaveFirstValidationError()
        {
            //Arrange

            //Act
            inizializedVideoValidationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> {
                { ValidationResults.ValidationError.Generic, "Generic Error" },
                { ValidationResults.ValidationError.InvalidSourceVideo, "Invalid Source Video" }
            });


            //Assert
            Assert.False(inizializedVideoValidationResult.IsValid);
            Assert.True(inizializedVideoValidationResult.Checked);
            Assert.Contains(inizializedVideoValidationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.Generic &&
                    i.ErrorMessage.Equals("Generic Error", StringComparison.Ordinal));
            Assert.Contains(inizializedVideoValidationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.InvalidSourceVideo &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.NotNull(inizializedVideoValidationResult.LastCheck);
        }

        [Fact]
        public void SetResult_AppendError_WhenHaveOtherValidationError()
        {
            //Arrange
            inizializedVideoValidationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> {
                { ValidationResults.ValidationError.Generic, "Generic Error" },
                { ValidationResults.ValidationError.InvalidSourceVideo, "Invalid Source Video" }
            });
            var firstLastCheck = inizializedVideoValidationResult.LastCheck;


            //Act
            inizializedVideoValidationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> {
                { ValidationResults.ValidationError.JsonConvert, "Json Convert" }
            });


            //Assert
            Assert.False(inizializedVideoValidationResult.IsValid);
            Assert.True(inizializedVideoValidationResult.Checked);
            Assert.Contains(inizializedVideoValidationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.Generic &&
                    i.ErrorMessage.Equals("Generic Error", StringComparison.Ordinal));
            Assert.Contains(inizializedVideoValidationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.InvalidSourceVideo &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.Contains(inizializedVideoValidationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.JsonConvert &&
                    i.ErrorMessage.Equals("Json Convert", StringComparison.Ordinal));
            Assert.NotNull(inizializedVideoValidationResult.LastCheck);
            Assert.NotEqual(firstLastCheck, inizializedVideoValidationResult.LastCheck);
        }

        [Fact]
        public void SetResult_ClearError_WhenIsValid()
        {
            //Arrange
            inizializedVideoValidationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> { { ValidationResults.ValidationError.Generic, "Generic Error" } });


            //Act
            inizializedVideoValidationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string>());


            //Assert
            Assert.True(inizializedVideoValidationResult.IsValid);
            Assert.True(inizializedVideoValidationResult.Checked);
            Assert.Empty(inizializedVideoValidationResult.ErrorValidationResults);
            Assert.NotNull(inizializedVideoValidationResult.LastCheck);
        }
    }
}
