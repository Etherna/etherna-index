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

using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class UserDto
    {
        // Constructors.
        public UserDto(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            Address = user.Address;
            CreationDateTime = user.CreationDateTime;
            IdentityManifest = user.IdentityManifest?.Hash;
        }

        // Properties.
        public string Address { get; }
        public DateTime CreationDateTime { get; }
        public string? IdentityManifest { get; }
    }
}
