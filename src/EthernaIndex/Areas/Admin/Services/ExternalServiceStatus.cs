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

namespace Etherna.EthernaIndex.Areas.Admin.Services
{
    /// <summary>
    /// The state of an external service the index depends on, as the index sees it.
    /// </summary>
    public sealed class ExternalServiceStatus(
        string name,
        string address,
        ExternalServiceProbe reachability,
        ExternalServiceProbe authentication)
    {
        // Properties.
        /// <summary>
        /// The address the index is configured to call.
        /// </summary>
        public string Address { get; } = address;

        /// <summary>
        /// Whether the index is accepted by the service as an authenticated client.
        /// </summary>
        public ExternalServiceProbe Authentication { get; } = authentication;

        public string Name { get; } = name;

        /// <summary>
        /// Whether the service answers at all, without authentication.
        /// </summary>
        public ExternalServiceProbe Reachability { get; } = reachability;
    }
}
