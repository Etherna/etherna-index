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

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public class ErrorDetail : ModelBase
    {
        // Constructors.
        public ErrorDetail(
            ValidationErrorType errorType,
            string? errorMessage = null)
        {
            ErrorType = errorType;
            ErrorMessage = errorMessage ?? errorType.ToString();
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
