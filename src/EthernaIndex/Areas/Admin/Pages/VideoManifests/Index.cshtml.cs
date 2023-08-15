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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class IndexModel : PageModel
    {
        // Models.
        public class VideoManifestDto
        {
            public VideoManifestDto(
                string manifestHash,
                string title)
            {
                if (manifestHash is null)
                    throw new ArgumentNullException(nameof(manifestHash));
                if (title is null)
                    throw new ArgumentNullException(nameof(title));

                ManifestHash = manifestHash;
                Title = title;
            }

            public string ManifestHash { get; }
            public string Title { get; }
        }

        // Consts.
        private const int PageSize = 20;

        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public IndexModel(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
            ErrorMessage = "";
        }

        // Properties.
        public int CurrentPage { get; private set; }
        public string ErrorMessage { get; private set; }
        public long MaxPage { get; private set; }
        public IEnumerable<VideoManifestDto> VideoManifests { get; set; } = default!;

        // Methods.
        public Task<IActionResult> OnGetAsync(
            string manifestHash,
            int? p) =>
            InitializeAsync(
                manifestHash,
                p);

        // Helpers.
        private async Task<IActionResult> InitializeAsync(
            string? manifestHash,
            int? p)
        {
            CurrentPage = p ?? 0;
            if (!string.IsNullOrWhiteSpace(manifestHash))
            {
                var videoManifests = await indexDbContext.VideoManifests.TryFindOneAsync(v => v.Manifest.Hash == manifestHash);

                if (videoManifests is not null)
                    return RedirectToPage("../VideoManifests/Manifest", new Dictionary<string, string> { { "manifestHash", manifestHash } });
            }
            else
            {
                var paginatedVideoManifests = await indexDbContext.VideoManifests.QueryPaginatedElementsAsync(
                vm => VideoWhere(vm, manifestHash),
                vm => vm.Id,
                CurrentPage,
                PageSize);

                MaxPage = paginatedVideoManifests.MaxPage;

                VideoManifests = paginatedVideoManifests.Elements.Select(
                    e => new VideoManifestDto(
                        e.Manifest.Hash,
                    e.TryGetTitle() ?? ""));
            }

            if (!string.IsNullOrWhiteSpace(manifestHash))
            {
                VideoManifests = Array.Empty<VideoManifestDto>();
                ErrorMessage = "ManifestHash not found.";
            }

            return new PageResult();
        }

        private IMongoQueryable<VideoManifest> VideoWhere(
            IMongoQueryable<VideoManifest> querable,
            string? manifestHash)
        {
            if (!string.IsNullOrWhiteSpace(manifestHash))
                querable = querable.Where(v => v.Manifest.Hash == manifestHash);

            return querable;
        }
    }
}
