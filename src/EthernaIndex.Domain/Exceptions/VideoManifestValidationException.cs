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
