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
