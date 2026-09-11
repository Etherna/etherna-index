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

using Etherna.EthernaIndex.Areas.Admin.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.Services
{
    public class IndexModel(IExternalServiceChecker externalServiceChecker) : PageModel
    {
        // Consts.
        public const int DefaultRefreshSeconds = 30;
        public static readonly int[] SelectableRefreshSeconds = [0, 15, 30, 60];

        // Properties.
        public DateTimeOffset CheckedAt { get; private set; }
        public int RefreshSeconds { get; private set; }
        public IEnumerable<ExternalServiceStatus> ServiceStatuses { get; private set; } = [];

        // Methods.
        public async Task OnGetAsync(int? refresh, CancellationToken cancellationToken)
        {
            RefreshSeconds = refresh.HasValue && SelectableRefreshSeconds.Contains(refresh.Value)
                ? refresh.Value
                : DefaultRefreshSeconds;

            ServiceStatuses = await externalServiceChecker.CheckAllAsync(cancellationToken);
            CheckedAt = DateTimeOffset.UtcNow;
        }
    }
}
