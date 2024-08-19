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

using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class ValidationError : ModelBase
    {
        // Constructors.
        public ValidationError(
            ValidationErrorType errorType,
            string? errorMessage = null)
        {
            ErrorType = errorType;
            ErrorMessage = errorMessage ?? errorType.ToString();
        }
#pragma warning disable CS8618 //Used only by EthernaIndex.Persistence
        protected ValidationError() { }
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
                EqualityComparer<string>.Default.Equals(ErrorMessage, (obj as ValidationError)!.ErrorMessage) &&
                ErrorType.Equals((obj as ValidationError)?.ErrorType);
        }

        public override int GetHashCode() =>
            (ErrorMessage ?? "").GetHashCode(StringComparison.OrdinalIgnoreCase) ^ ErrorType.GetHashCode();
    }
}
