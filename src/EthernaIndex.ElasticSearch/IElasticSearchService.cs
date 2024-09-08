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

using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public interface IElasticSearchService
    {
        Task IndexCommentAsync(Comment comment);
        Task IndexVideoAsync(Video video);
        Task RemoveCommentIndexAsync(Comment comment);
        Task RemoveCommentIndexAsync(string commentId);
        Task RemoveVideoIndexAsync(Video video);
        Task RemoveVideoIndexAsync(string videoId);
        Task<(IEnumerable<VideoDocument> Results, long TotalElements)> SearchVideoAsync(string query, int page, int take);
    }
}
