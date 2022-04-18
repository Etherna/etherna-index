using Etherna.EthernaIndex.Domain.Models.VideoAgg;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
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
