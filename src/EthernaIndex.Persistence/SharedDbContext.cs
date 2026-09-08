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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Persistence.Repositories;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Repositories;
using Etherna.Scrinium.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Etherna.EthernaIndex.Persistence
{
    public class SharedDbContext : DbContext, ISharedDbContext
    {
        // Consts.
        private const string ModelMapsNamespace = "Etherna.EthernaIndex.Persistence.ModelMaps.SsoShared";

        // Properties.
        //repositories
        //no index builders: the SSO owns the collection and its indexes, this db context is read only
        public IRepository<UserSharedInfo, string> UsersInfo { get; } =
            new DomainRepository<UserSharedInfo, string>("usersInfo");

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(SharedDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == ModelMapsNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;
    }
}
