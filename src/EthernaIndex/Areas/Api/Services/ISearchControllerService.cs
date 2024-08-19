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

using Etherna.EthernaIndex.Areas.Api.DtoModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public interface ISearchControllerService
    {
        void ReindexAllVideos();
        Task<PaginatedEnumerableDto<VideoPreviewDto>> SearchVideoAsync(string query, int page, int take);

        //deprecated
        [Obsolete("Used only for API backwards compatibility")]
        Task<IEnumerable<VideoDto>> SearchVideoAsync_old(string query, int page, int take);
    }
}
