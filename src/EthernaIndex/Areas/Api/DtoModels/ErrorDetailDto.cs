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

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class ErrorDetailDto
    {
        // Constructors.
        public ErrorDetailDto(
            string errorMessage,
            ValidationErrorType errorType)
        {
            ErrorMessage = errorMessage;
            ErrorType = errorType;

#pragma warning disable CS0618 // Type or member is obsolete
            ErrorNumber = errorType;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        // Properties.
        public virtual string ErrorMessage { get; private set; }
        [Obsolete("Use ErrorType instead")]
        public virtual ValidationErrorType ErrorNumber { get; private set; }
        public virtual ValidationErrorType ErrorType { get; private set; }
    }
}
