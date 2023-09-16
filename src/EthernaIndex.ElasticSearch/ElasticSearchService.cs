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
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public class ElasticSearchService : IElasticSearchService
    {
        // Fields.
        private readonly IElasticClient elasticClient;
        private readonly ISharedDbContext sharedDbContext;

        // Constructors.
        public ElasticSearchService(
            IElasticClient elasticClient,
            ISharedDbContext sharedDbContext)
        {
            this.elasticClient = elasticClient;
            this.sharedDbContext = sharedDbContext;
        }

        // Public methods.
        public async Task IndexCommentAsync(Comment comment)
        {
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));

            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(comment.Author.SharedInfoId);

            var document = new CommentDocument(comment, ownerSharedInfo);

            await elasticClient.IndexDocumentAsync(document);
        }

        public async Task IndexVideoAsync(Video video)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            if (video.LastValidManifest is null)
                throw new InvalidOperationException($"{nameof(video.LastValidManifest)} can't be null");

            var document = new VideoDocument(video);

            await elasticClient.IndexDocumentAsync(document);
        }

        public async Task RemoveCommentIndexAsync(Comment comment)
        {
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));

            await RemoveCommentIndexAsync(comment.Id);
        }

        public async Task RemoveCommentIndexAsync(string commentId) =>
            await elasticClient.DeleteAsync<CommentDocument>(commentId);

        public async Task RemoveVideoIndexAsync(Video video)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            await RemoveVideoIndexAsync(video.Id);
        }

        public async Task RemoveVideoIndexAsync(string videoId) =>
            await elasticClient.DeleteAsync<VideoDocument>(videoId);

        public async Task<IEnumerable<string>> SearchVideoIndexBeforeAsync(DateTime updateDate)
        {
            var searchResponse = await elasticClient.SearchAsync<VideoDocument>(s => s
                .Query(q => q
                    .DateRange(r => r
                        .Field(f => f.IndexingDateTime)
                            .LessThan(updateDate)))
                .Source(sf => sf
                    .Includes(i => i
                        .Field(f => f.Id))
                ));

            return searchResponse.Documents.Select(d => d.Id);
        }

        public async Task<(IEnumerable<VideoDocument> Results, long TotalElements)> SearchVideoAsync(string query, int page, int take)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(query);
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (take <= 0)
                throw new ArgumentOutOfRangeException(nameof(take));


            var searchResponse = await elasticClient.SearchAsync<VideoDocument>(s =>
                s.Query(q => q.Bool(b =>
                    b.Must(mu => mu.Wildcard(f => f.Title, $"*{query.ToLowerInvariant()}*") ||
                    mu.Wildcard(f => f.Description, $"*{query.ToLowerInvariant()}*"))
                ))
                .From(page * take)
                .Size(take)
                .TrackTotalHits(true));

            return (searchResponse.Documents, searchResponse.Total);
        }
    }
}
