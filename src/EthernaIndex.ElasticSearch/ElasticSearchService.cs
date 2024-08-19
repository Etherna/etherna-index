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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Nest;
using System;
using System.Collections.Generic;
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
            ArgumentNullException.ThrowIfNull(comment, nameof(comment));

            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(comment.Author.SharedInfoId);

            var document = new CommentDocument(comment, ownerSharedInfo);

            await elasticClient.IndexDocumentAsync(document);
        }

        public async Task IndexVideoAsync(Video video)
        {
            ArgumentNullException.ThrowIfNull(video, nameof(video));
            if (video.LastValidManifest is null)
                throw new InvalidOperationException($"{nameof(video.LastValidManifest)} can't be null");

            var document = new VideoDocument(video);

            await elasticClient.IndexDocumentAsync(document);
        }

        public async Task RemoveCommentIndexAsync(Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment, nameof(comment));

            await RemoveCommentIndexAsync(comment.Id);
        }

        public async Task RemoveCommentIndexAsync(string commentId) =>
            await elasticClient.DeleteAsync<CommentDocument>(commentId);

        public async Task RemoveVideoIndexAsync(Video video)
        {
            ArgumentNullException.ThrowIfNull(video, nameof(video));

            await RemoveVideoIndexAsync(video.Id);
        }

        public async Task RemoveVideoIndexAsync(string videoId) =>
            await elasticClient.DeleteAsync<VideoDocument>(videoId);

        public async Task<(IEnumerable<VideoDocument> Results, long TotalElements)> SearchVideoAsync(string query, int page, int take)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(query);
            ArgumentOutOfRangeException.ThrowIfNegative(page, nameof(page));
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take, nameof(take));


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
