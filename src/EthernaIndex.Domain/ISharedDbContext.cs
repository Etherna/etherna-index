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

using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Repositories;

namespace Etherna.EthernaIndex.Domain
{
    /// <summary>
    /// Shared DbContext between Etherna services. It's managed by SSO Server, use in read-only mode.
    /// </summary>
    public interface ISharedDbContext : IDbContext
    {
        IRepository<UserSharedInfo, string> UsersInfo { get; }
    }
}
