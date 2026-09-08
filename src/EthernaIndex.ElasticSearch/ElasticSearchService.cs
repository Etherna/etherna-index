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

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;
using Etherna.EthernaIndex.ElasticSearch.Options;
using Etherna.MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public class ElasticSearchService(
        ElasticsearchClient client,
        IIndexDbContext indexDbContext,
        IOptions<ElasticSearchOptions> options)
        : IElasticSearchService
    {
        // Consts.
        /// <summary>
        /// Defensive bound on the amount of comment entities loaded from the database while
        /// filling <see cref="MaxIndexedCommentsTotalChars"/>, to keep memory usage bounded.
        /// </summary>
        public const int MaxIndexedComments = 10_000;

        /// <summary>
        /// Budget on the total amount of characters of comment text denormalized into a video
        /// document, to keep its size bounded on pathological videos (e.g. massive spam).
        /// Comments are included newest-first until the budget is filled; any realistic video
        /// fits entirely.
        /// </summary>
        public const int MaxIndexedCommentsTotalChars = 1_000_000;

        public const string VideosIndexBaseName = "video";

        // Fields.
        private readonly ElasticSearchOptions options = options.Value;

        // Methods.
        public async Task AddVideoAsync(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);
            if (video.LastValidManifest is null)
                throw new InvalidOperationException($"{nameof(video.LastValidManifest)} can't be null");

            // The manifest summary carries no metadata: preload it before building the document.
            await indexDbContext.LoadValuesAsync(video.LastValidManifest, m => m.Metadata);

            // Frozen comments are excluded because their text has been replaced by a removal placeholder.
            var comments = await indexDbContext.Comments.QueryElementsAsync(elements =>
                elements.Where(c => c.Video.Id == video.Id)
                    .Where(c => !c.IsFrozen)
                    .OrderByDescending(c => c.CreationDateTime)
                    .Take(MaxIndexedComments)
                    .ToListAsync());

            // Denormalize comment texts newest-first, until the character budget is filled.
            var commentTexts = new List<string>();
            var totalChars = 0;
            foreach (var comment in comments)
            {
                var text = comment.LastText;
                if (totalChars + text.Length > MaxIndexedCommentsTotalChars)
                    break;
                totalChars += text.Length;
                commentTexts.Add(text);
            }

            var document = new VideoDocument(video, commentTexts);

            // Set the document id explicitly: the overload taking only the index name would let
            // Elasticsearch auto-generate an id, so re-adding the same video (e.g. on every new
            // comment) would create a duplicate document instead of overwriting it.
            var response = await client.IndexAsync(document, i => i
                .Index(options.VideosIndexName)
                .Id(document.Id));
            if (!response.IsValidResponse &&
                response.TryGetOriginalException(out var exception) &&
                exception is not null)
                throw exception;
        }

        public async Task CreateIndexesAsync()
        {
            await client.Indices.CreateAsync<VideoDocument>(options.VideosIndexName,
                index => index.Mappings(maps =>
                    maps.Properties(props =>
                    {
                        props.Text(v => v.Id);
                        props.Text(v => v.Comments);
                        props.Date(v => v.CreationDateTime);
                        props.Date(v => v.IndexingDateTime);
                        props.Text(v => v.Description);
                        props.LongNumber(v => v.Duration);
                        props.Boolean(v => v.IsFrozen);
                        props.Text(v => v.ManifestReference);
                        props.Text(v => v.OwnerSharedInfoId);
                        props.Object(v => v.Thumbnail, thumbConf =>
                        {
                            thumbConf.Properties(tProps =>
                            {
                                tProps.FloatNumber(v => v.Thumbnail.AspectRatio);
                                tProps.Text(v => v.Thumbnail.Blurhash);
                                tProps.Object(v => v.Thumbnail.Sources, tSourceConf =>
                                {
                                    tSourceConf.Properties(tsProps =>
                                    {
                                        tsProps.IntegerNumber(v => v.Thumbnail.Sources.First().Width);
                                        tsProps.Text(v => v.Thumbnail.Sources.First().Path);
                                        tsProps.Text(v => v.Thumbnail.Sources.First().Type);
                                    });
                                });
                            });
                        });
                        props.Text(v => v.Title);
                        props.LongNumber(v => v.TotDownvotes);
                        props.LongNumber(v => v.TotUpvotes);
                    })));
        }

        public async Task DeleteVideoAsync(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);

            await client.DeleteAsync<VideoDocument>(video.Id);
        }

        public async Task DestroyIndexesAsync()
        {
            await client.Indices.DeleteAsync(
                options.VideosIndexName,
                d => d.IgnoreUnavailable());
        }

        public Task<long> RemoveVideoDocumentsIndexedBeforeAsync(DateTime threshold) =>
            RemoveDocumentsIndexedBeforeAsync<VideoDocument>(
                options.VideosIndexName, v => v.IndexingDateTime, threshold);

        public async Task<(IEnumerable<VideoDocument> Results, long TotalElements)> SearchVideoAsync(
            string query,
            int page,
            int take)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException(query);
            ArgumentOutOfRangeException.ThrowIfNegative(page);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

            var searchResponse = await client.SearchAsync<VideoDocument>(s =>
                s.Indices(options.VideosIndexName)
                    .Query(q => q.Bool(b =>
                        b.Should(sh => sh.SimpleQueryString(sq =>
                            {
                                sq.Query(query);
                                sq.Fields(Fields.FromFields(
                                [
                                    new Field("title", 2),
                                    new Field("description"),
                                    new Field("comments", 0.5)
                                ]));
                                sq.DefaultOperator(Operator.Or);
                            }))
                            .MinimumShouldMatch(1)
                    ))
                    .From(page * take)
                    .Size(take)
                    .TrackTotalHits(new TrackHits(true)));

            if (!searchResponse.IsValidResponse)
            {
                if (searchResponse.TryGetOriginalException(out var exception) &&
                    exception is not null)
                    throw exception;
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return (searchResponse.Documents, searchResponse.Total);
        }

        // Helpers.
        private async Task<long> RemoveDocumentsIndexedBeforeAsync<TDocument>(
            string indexName,
            Expression<Func<TDocument, object?>> indexingDateTimeField,
            DateTime threshold)
            where TDocument : class
        {
            /* Make the documents just indexed visible before pruning: without a refresh the delete
             * query snapshot still sees them with their previous indexing stamp, and deleting a
             * document changed after the snapshot is a version conflict, refused with a 409 by
             * default. Documents updated during the prune, by an event handler indexing them, are
             * skipped instead of aborting it: a document just rewritten is fresh by definition. */
            var refreshResponse = await client.Indices.RefreshAsync(Indices.Index(indexName));
            if (!refreshResponse.IsValidResponse)
                throw new InvalidOperationException(refreshResponse.DebugInformation);

            var response = await client.DeleteByQueryAsync<TDocument>(
                Indices.Index(indexName),
                d => d.Conflicts(Conflicts.Proceed)
                    .Query(q => q.Range(r => r.Date(dr => dr
                        .Field(indexingDateTimeField)
                        .Lt(threshold)))));

            if (!response.IsValidResponse)
            {
                if (response.TryGetOriginalException(out var exception) &&
                    exception is not null)
                    throw exception;
                throw new InvalidOperationException(response.DebugInformation);
            }

            return response.Deleted ?? 0;
        }
    }
}
