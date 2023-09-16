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

using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using System;
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
        Task<IEnumerable<string>> SearchVideoIndexBeforeAsync(DateTime updateDate);
        Task<(IEnumerable<VideoDocument> Results, long TotalElements)> SearchVideoAsync(string query, int page, int take);
    }
}
