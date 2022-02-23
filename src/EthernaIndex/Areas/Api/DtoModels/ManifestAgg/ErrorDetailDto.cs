using Etherna.EthernaIndex.Domain.Models.ManifestAgg;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels.ManifestAgg
{
    public class ErrorDetailDto
    {
        // Constructors.
        public ErrorDetailDto(
            string errorMessage,
            ValidationErrorType errorNumber)
        {
            ErrorMessage = errorMessage;
            ErrorNumber = errorNumber;
        }

        // Properties.
        public virtual string ErrorMessage { get; private set; }
        public virtual ValidationErrorType ErrorNumber { get; private set; }
    }
}
