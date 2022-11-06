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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.ElasticSearch.Configs
{
    public class ElasticSearchOptions
    {
        // Consts.
        private const string commentsIndexBaseName = "comments";
        private const string videosIndexBaseName = "videos";

        // Constructor.
        public ElasticSearchOptions(IEnumerable<string> urls)
        {
            if (urls is null)
                throw new ArgumentNullException(nameof(urls));
            if (!urls.Any())
                throw new ArgumentException("Urls arg can't be empty", nameof(urls));

            Urls = urls;
        }

        // Properties.
        public string CommentsIndexName => (IndexesPrefix ?? "") + commentsIndexBaseName;
        public string? IndexesPrefix { get; set; }
        public IEnumerable<string> Urls { get; }
        public string VideosIndexName => (IndexesPrefix ?? "") + videosIndexBaseName;
    }
}
