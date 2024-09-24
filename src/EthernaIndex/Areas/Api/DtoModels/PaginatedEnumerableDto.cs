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

using Etherna.MongODM.Core.Repositories;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class PaginatedEnumerableDto<TModel>
    {
        // Constructors.
        public PaginatedEnumerableDto(
            PaginatedEnumerable<TModel> paginatedEnumerable)
        {
            ArgumentNullException.ThrowIfNull(paginatedEnumerable, nameof(paginatedEnumerable));

            CurrentPage = paginatedEnumerable.CurrentPage;
            Elements = paginatedEnumerable.Elements;
            MaxPage = paginatedEnumerable.MaxPage;
            PageSize = paginatedEnumerable.PageSize;
            TotalElements = paginatedEnumerable.TotalElements;
        }

        public PaginatedEnumerableDto(
            int currentPage,
            IEnumerable<TModel> elements,
            int pageSize,
            long totalElements)
        {
            CurrentPage = currentPage;
            Elements = elements;
            MaxPage = (totalElements - 1) / pageSize;
            PageSize = pageSize;
            TotalElements = totalElements;
        }

        // Properties.
        public int CurrentPage { get; }
        public IEnumerable<TModel> Elements { get; }
        public long MaxPage { get; }
        public int PageSize { get; }
        public long TotalElements { get; }
    }
}
