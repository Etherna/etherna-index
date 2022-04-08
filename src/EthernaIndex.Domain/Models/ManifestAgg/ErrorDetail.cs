﻿using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.ManifestAgg
{
    public class ErrorDetail : ModelBase
    {
        // Constructors.
        public ErrorDetail(
            ValidationErrorType errorNumber,
            string errorMessage)
        {
            ErrorType = errorNumber;
            ErrorMessage = errorMessage;
        }
#pragma warning disable CS8618 //Used only by EthernaIndex.Persistence
        protected ErrorDetail() { }
#pragma warning restore CS8618 //Used only by EthernaIndex.Persistence

        // Properties.
        public virtual string ErrorMessage { get; protected set; }
        public virtual ValidationErrorType ErrorType { get; protected set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<string>.Default.Equals(ErrorMessage, (obj as ErrorDetail)!.ErrorMessage) &&
                ErrorType.Equals((obj as ErrorDetail)?.ErrorType);
        }

        public override int GetHashCode() =>
            (ErrorMessage ?? "").GetHashCode(StringComparison.OrdinalIgnoreCase) ^ ErrorType.GetHashCode();
    }
}
