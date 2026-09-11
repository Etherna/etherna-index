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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Services
{
    public interface IExternalServiceChecker
    {
        /// <summary>
        /// Check every external service the index depends on at runtime.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The state of each service, in the order they are checked</returns>
        Task<IEnumerable<ExternalServiceStatus>> CheckAllAsync(CancellationToken cancellationToken = default);
    }
}
