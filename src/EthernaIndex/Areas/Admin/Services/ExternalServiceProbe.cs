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

namespace Etherna.EthernaIndex.Areas.Admin.Services
{
    /// <summary>
    /// The outcome of a single check performed against an external service.
    /// </summary>
    public sealed class ExternalServiceProbe(
        string description,
        TimeSpan duration,
        string? detail = null,
        string? error = null)
    {
        // Properties.
        /// <summary>
        /// What the check did, as an endpoint or an operation.
        /// </summary>
        public string Description { get; } = description;

        /// <summary>
        /// What the service answered, when the check succeeded.
        /// </summary>
        public string? Detail { get; } = detail;

        public TimeSpan Duration { get; } = duration;

        /// <summary>
        /// Why the check failed. Null when it succeeded.
        /// </summary>
        public string? Error { get; } = error;

        public bool Succeeded => Error is null;
    }
}
