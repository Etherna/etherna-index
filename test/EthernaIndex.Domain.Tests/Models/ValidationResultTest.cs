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
        [Fact]
        public void Create_ValidationResult_WithDefaultValue()
        {
            //Arrange
            var hash = "testHash";


            //Act
            var validationResult = new ValidationResult(hash);


            //Assert
            Assert.Equal(hash, validationResult.ManifestHash.Hash);
            Assert.Null(validationResult.IsValid);
            Assert.Null(validationResult.LastCheck);
        }

        [Fact]
        public void SetResult_InsertError_WhenHaveFirstValidationError()
        {
            //Arrange
            var hash = "testHash";
            var validationResult = new ValidationResult(hash);


            //Act
            validationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> {
                { ValidationResults.ValidationError.Generic, "Generic Error" },
                { ValidationResults.ValidationError.InvalidSourceVideo, "Invalid Source Video" }
            });


            //Assert
            Assert.False(validationResult.IsValid);
            Assert.True(validationResult.Checked);
            Assert.Contains(validationResult.ErrorValidationResults, 
                i => i.ErrorNumber == ValidationResults.ValidationError.Generic &&
                    i.ErrorMessage.Equals("Generic Error", StringComparison.Ordinal));
            Assert.Contains(validationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.InvalidSourceVideo &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.NotNull(validationResult.LastCheck);
        }

        [Fact]
        public void SetResult_AppendError_WhenHaveOtherValidationError()
        {
            //Arrange
            var hash = "testHash";
            var validationResult = new ValidationResult(hash);
            validationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> {
                { ValidationResults.ValidationError.Generic, "Generic Error" },
                { ValidationResults.ValidationError.InvalidSourceVideo, "Invalid Source Video" }
            });
            var firstLastCheck = validationResult.LastCheck;


            //Act
            validationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> {
                { ValidationResults.ValidationError.JsonConvert, "Json Convert" }
            });


            //Assert
            Assert.False(validationResult.IsValid);
            Assert.True(validationResult.Checked);
            Assert.Contains(validationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.Generic &&
                    i.ErrorMessage.Equals("Generic Error", StringComparison.Ordinal));
            Assert.Contains(validationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.InvalidSourceVideo &&
                    i.ErrorMessage.Equals("Invalid Source Video", StringComparison.Ordinal));
            Assert.Contains(validationResult.ErrorValidationResults,
                i => i.ErrorNumber == ValidationResults.ValidationError.JsonConvert &&
                    i.ErrorMessage.Equals("Json Convert", StringComparison.Ordinal));
            Assert.NotNull(validationResult.LastCheck);
            Assert.NotEqual(firstLastCheck, validationResult.LastCheck); 
        }

        [Fact]
        public void SetResult_ClearError_WhenIsValid()
        {
            //Arrange
            var hash = "testHash";
            var validationResult = new ValidationResult(hash);
            validationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string> { { ValidationResults.ValidationError.Generic, "Generic Error" } });


            //Act
            validationResult.SetResult(new Dictionary<ValidationResults.ValidationError, string>());


            //Assert
            Assert.True(validationResult.IsValid);
            Assert.True(validationResult.Checked);
            Assert.Empty(validationResult.ErrorValidationResults);
            Assert.NotNull(validationResult.LastCheck);
        }
    }
}
