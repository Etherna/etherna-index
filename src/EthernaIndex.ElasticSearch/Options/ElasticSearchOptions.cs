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

namespace Etherna.EthernaIndex.ElasticSearch.Options
{
    public class ElasticSearchOptions
    {
        // Properties.
        public string? IndexesPrefix { get; set; }
        public string? Password { get; set; }
        public IEnumerable<string> Urls { get; set; } = [];
        public string? Username { get; set; }
        
        public string CommentsIndexName => (IndexesPrefix ?? "") + ElasticSearchService.CommentsIndexBaseName;
        public string VideosIndexName => (IndexesPrefix ?? "") + ElasticSearchService.VideosIndexBaseName;
    }
}
