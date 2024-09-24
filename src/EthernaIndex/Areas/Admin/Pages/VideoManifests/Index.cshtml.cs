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

using Etherna.BeeNet.Models;
using Etherna.EthernaIndex.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class IndexModel(IIndexDbContext indexDbContext) : PageModel
    {
        // Models.
        public class VideoManifestDto(SwarmHash manifestHash, string title)
        {
            public SwarmHash ManifestHash { get; } = manifestHash;
            public string Title { get; } = title;
        }

        // Consts.
        private const int PageSize = 20;

        // Properties.
        public int CurrentPage { get; private set; }
        public string ErrorMessage { get; private set; } = "";
        public long MaxPage { get; private set; }
        public IEnumerable<VideoManifestDto> VideoManifests { get; set; } = default!;

        // Methods.
        public async Task<IActionResult> OnGetAsync(
            SwarmHash? manifestHash,
            int? p)
        {
            CurrentPage = p ?? 0;
            if (manifestHash.HasValue)
            {
                var videoManifests = await indexDbContext.VideoManifests.TryFindOneAsync(v => v.ManifestHash == manifestHash);

                if (videoManifests is not null)
                    return RedirectToPage("Manifest", new { manifestHash });

                VideoManifests = Array.Empty<VideoManifestDto>();
                ErrorMessage = "ManifestHash not found.";
            }
            else
            {
                var paginatedVideoManifests = await indexDbContext.VideoManifests.QueryPaginatedElementsAsync(
                    vm => vm,
                    vm => vm.Id,
                    CurrentPage,
                    PageSize);

                MaxPage = paginatedVideoManifests.MaxPage;

                VideoManifests = paginatedVideoManifests.Elements.Select(
                    e => new VideoManifestDto(
                        e.ManifestHash,
                    e.TryGetTitle() ?? ""));
            }

            return new PageResult();
        }
    }
}
