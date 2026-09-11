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

using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.SwarmSdk.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class UserDto
    {
        // Constructors.
        public UserDto(User user, UserSharedInfo? userSharedInfo)
        {
            ArgumentNullException.ThrowIfNull(user);

            Id = user.Id;
            Address = userSharedInfo?.EtherAddress;
            CreationDateTime = user.CreationDateTime;
        }

        // Properties.
        public string Id { get; }
        public EthAddress? Address { get; }
        public DateTime CreationDateTime { get; }
    }
}
