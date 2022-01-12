using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Domain.Models.ValidationResults
{
    public class ErrorValidationResult
    {
        // Constructors.
        public ErrorValidationResult(
            ValidationError errorNumber,
            string errorMessage,
            DateTime checkedTime)
        {
            ErrorNumber = errorNumber;
            ErrorMessage = errorMessage;
            CheckedTime = checkedTime;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ErrorValidationResult() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


        // Properties.
        public virtual DateTime CheckedTime { get; protected set; }
        public virtual string ErrorMessage { get; protected set; }
        public virtual ValidationError ErrorNumber { get; protected set; }
    }
}
