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

using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Exceptions
{
    [SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Validation errors are required")]
    public class VideoManifestValidationException : Exception
    {
        // Constructors.
        public VideoManifestValidationException()
            : this(Array.Empty<ValidationError>())
        { }

        public VideoManifestValidationException(IEnumerable<ValidationError> validationErrors)
            : base(ValidationErrosToString(validationErrors))
        {
            ValidationErrors = validationErrors;
        }

        public VideoManifestValidationException(IEnumerable<ValidationError> validationErrors, Exception innerException)
            : base(ValidationErrosToString(validationErrors), innerException)
        {
            ValidationErrors = validationErrors;
        }

        // Properties.
        public IEnumerable<ValidationError> ValidationErrors { get; }

        // Helpers.
        private static string ValidationErrosToString(IEnumerable<ValidationError> validationErrors) =>
            validationErrors is null ? "" :
            validationErrors.Aggregate(
                "",
                (a, e) =>
                {
                    if (!string.IsNullOrEmpty(a))
                        a += "\n";
                    a += $"{e.ErrorType}: {e.ErrorMessage}";
                    return a;
                });
    }
}
