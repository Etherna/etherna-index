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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.ElasticSearch
{
    public class ElasticSearchService(
        ElasticsearchClient client,
        IOptions<ElasticSearchOptions> options,
        ISharedDbContext sharedDbContext)
        : IElasticSearchService
    {
        // Consts.
        public const string CommentsIndexBaseName = "comments";
        public const string VideosIndexBaseName = "videos";
        
        // Fields.
        private readonly ElasticSearchOptions options = options.Value;
        
        // Methods.
        public async Task AddCommentAsync(Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);

            var ownerSharedInfo = await sharedDbContext.UsersInfo.FindOneAsync(comment.Author.SharedInfoId);
            var document = new CommentDocument(comment, ownerSharedInfo);

            var response = await client.IndexAsync(document, (IndexName)options.CommentsIndexName, null);
            if (!response.IsValidResponse &&
                response.TryGetOriginalException(out var exception) &&
                exception is not null)
                throw exception;
        }

        public async Task AddVideoAsync(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);
            if (video.LastValidManifest is null)
                throw new InvalidOperationException($"{nameof(video.LastValidManifest)} can't be null");

            var document = new VideoDocument(video);

            var response = await client.IndexAsync(document, (IndexName)options.VideosIndexName, null);
            if (!response.IsValidResponse &&
                response.TryGetOriginalException(out var exception) &&
                exception is not null)
                throw exception;
        }

        public async Task CreateIndexesAsync()
        {
            await client.Indices.CreateAsync<CommentDocument>(options.CommentsIndexName,
                index => index.Mappings(maps =>
                    maps.Properties(props =>
                    {
                        props.Text(c => c.Id);
                        props.Date(c => c.CreationDateTime);
                        props.Boolean(c => c.IsFrozen);
                        props.Date(c => c.LastUpdateDateTime);
                        props.Text(c => c.OwnerAddress);
                        props.Text(c => c.Text);
                        props.Text(c => c.VideoId);
                    })));
            await client.Indices.CreateAsync<VideoDocument>(options.VideosIndexName,
                index => index.Mappings(maps =>
                    maps.Properties(props =>
                    {
                        props.Text(v => v.Id);
                        props.Date(v => v.CreationDateTime);
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

        public async Task DeleteCommentAsync(Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);

            await client.DeleteAsync<CommentDocument>(comment.Id);
        }

        public async Task DeleteVideoAsync(Video video)
        {
            ArgumentNullException.ThrowIfNull(video);

            await client.DeleteAsync<VideoDocument>(video.Id);
        }

        public async Task DestroyIndexesAsync()
        {
            await client.Indices.DeleteAsync(new[]
            {
                options.CommentsIndexName,
                options.VideosIndexName
            });
        }

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
                s.Query(q => q.Bool(b =>
                        b.Should(sh => sh.SimpleQueryString(sq =>
                            {
                                sq.Query(query);
                                sq.Fields(Fields.FromFields(
                                [
                                    new Field("title", 2),
                                    new Field("description")
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
    }
}
