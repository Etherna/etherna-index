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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class IndexModel : PageModel
    {
        // Models.
        public class InputModel
        {
            [Display(Name = "Manifest Hash")]
            public string? ManifestHash { get; set; }
        }

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
        }

        // Properties.
        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public int CurrentPage { get; private set; }
        public long MaxPage { get; private set; }
        public IEnumerable<VideoManifestDto> VideoManifests { get; set; } = default!;

        // Methods.
        public Task OnGetAsync(
            string manifestHash,
            int? p) =>
            InitializeAsync(
                manifestHash,
                p);

        public Task OnPost() =>
            InitializeAsync(
                Input?.ManifestHash,
                null);

        // Helpers.
        private async Task InitializeAsync(
            string? manifestHash,
            int? p)
        {
            CurrentPage = p ?? 0;

            var paginatedVideoManifests = await indexDbContext.VideoManifests.QueryPaginatedElementsAsync(
                vm => VideoWhere(vm, manifestHash),
                vm => vm.Id,
                CurrentPage,
                PageSize);

            MaxPage = paginatedVideoManifests.MaxPage;

            VideoManifests= paginatedVideoManifests.Elements.Select( 
                e => new VideoManifestDto(
                    e.Manifest.Hash, 
                    e.TryGetTitle() ?? ""));
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
