using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Domain.Models.Manifest
{
    public class ErrorDetail : ModelBase
    {
        // Constructors.
        public ErrorDetail(
            ValidationErrorType errorNumber,
            string errorMessage)
        {
            ErrorNumber = errorNumber;
            ErrorMessage = errorMessage;
        }
#pragma warning disable CS8618 //Used only by EthernaIndex.Persistence
        protected ErrorDetail() { }
#pragma warning restore CS8618 //Used only by EthernaIndex.Persistence


        // Properties.
        public virtual string ErrorMessage { get; protected set; }
        public virtual ValidationErrorType ErrorNumber { get; protected set; }
    }
}
