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

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class SettingsDto
    {
        // Constructors.
        public SettingsDto(
            string defaultSwarmGatewayUrl,
            string version)
        {
            DefaultSwarmGatewayUrl = defaultSwarmGatewayUrl ?? throw new System.ArgumentNullException(nameof(defaultSwarmGatewayUrl));
            Version = version ?? throw new System.ArgumentNullException(nameof(version));
        }

        // Properties.
        public string DefaultSwarmGatewayUrl { get; }
        public string Version { get; }
    }
}
