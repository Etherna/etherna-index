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

using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Persistence.Helpers;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver;
using Etherna.Scrinium.Core.Serialization.Serializers;
using Etherna.Scrinium.Core.Utility;
using Etherna.SwarmSdk.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    /// <summary>
    /// Round trip tests: a stored document deserialized and serialized back must come out in the
    /// shape the active schemas declare. They cover what a deserialization test can't see, a
    /// summary whose reference schema changed the element of a member since it was written.
    /// </summary>
    public class IndexDbContextSerializationTest
    {
        // Fields.
        private readonly IndexDbContext dbContext;
        private readonly Mock<IMongoDatabase> mongoDatabaseMock = new();

        // Constructor.
        public IndexDbContextSerializationTest()
        {
            // Setup dbContext.
            var eventDispatcherMock = new Mock<IEventDispatcher>();
            var loggerMock = new Mock<ILogger<IndexDbContext>>();
            dbContext = new IndexDbContext(eventDispatcherMock.Object, loggerMock.Object);

            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);
        }

        // Tests.
        [Fact]
        public void VideoReadsTheManifestPreviewWrittenByTheCurrentSchema()
        {
            /* The preview summary written by the current schema carries its denormalized members:
             * they read from the video document, without loading the manifest one. */

            // Setup.
            var sourceDocument =
                """
                {
                    "_id" : ObjectId("625df43c74679c25b6c157ec"),
                    "_s" : "d0c48dd8-0887-4ac5-80e5-9b08c5dc77f1",
                    "CreationDateTime" : ISODate("2022-04-18T23:29:00.840+0000"),
                    "IsFrozen" : false,
                    "LastValidManifest" : {
                        "_s" : "8c4b7808-bc93-47c5-a4f3-68c1ce666311",
                        "CreationDateTime" : ISODate("2022-04-18T23:29:00.919+0000"),
                        "_id" : ObjectId("625df43c74679c25b6c157ed"),
                        "IsValid" : true,
                        "ManifestReference" : "e15d54efa42840e74fde05137450f42e8c41f51f7541b1a6999eaa51fde04408"
                    },
                    "Owner" : {
                        "_s" : "caa0968f-4493-485b-b8d0-bc40942e8684",
                        "_id" : ObjectId("625df43c74679c25b6c157eb"),
                        "SharedInfoId" : "625da02c2752994b203d3681"
                    },
                    "TotDownvotes" : NumberLong(0),
                    "TotUpvotes" : NumberLong(0),
                    "VideoManifests" : [ ]
                }
                """;

            DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.Videos);
            DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.VideoManifests);
            DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.Users);

            var modelMapSerializer = new ModelMapSerializer<Video>(dbContext.Engine);
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext);

            // Action.
            using var documentReader = new JsonReader(sourceDocument);
            var video = modelMapSerializer.Deserialize(BsonDeserializationContext.CreateRoot(documentReader));

            // Assert.
            //the denormalized members read from the video document, through the active schema
            var manifestPreview = video.LastValidManifest!;
            Assert.Equal(
                new SwarmReference("e15d54efa42840e74fde05137450f42e8c41f51f7541b1a6999eaa51fde04408"),
                manifestPreview.ManifestReference);
            Assert.True(manifestPreview.IsValid);
            Assert.Equal("625df43c74679c25b6c157ed", manifestPreview.Id);
        }

        [Fact]
        public void VideoWritesTheManifestPreviewInItsCurrentShape()
        {
            /* The manifest reference of the preview summary was stored as "ManifestHash" until
             * v0.3.15, under the schema id the current shape kept until v0.3.20: a video stored
             * then deserializes through the secondary schema, and writes back the current
             * element name under the current schema id, without reading the origin document. */

            // Setup.
            var sourceDocument =
                """
                {
                    "_id" : ObjectId("625df43c74679c25b6c157ec"),
                    "_m" : "d0c48dd8-0887-4ac5-80e5-9b08c5dc77f1",
                    "CreationDateTime" : ISODate("2022-04-18T23:29:00.840+0000"),
                    "IsFrozen" : false,
                    "LastValidManifest" : {
                        "_m" : "f7966611-14aa-4f18-92f4-8697b4927fb6",
                        "CreationDateTime" : ISODate("2022-04-18T23:29:00.919+0000"),
                        "_id" : ObjectId("625df43c74679c25b6c157ed"),
                        "IsValid" : true,
                        "ManifestHash" : "e15d54efa42840e74fde05137450f42e8c41f51f7541b1a6999eaa51fde04408"
                    },
                    "Owner" : {
                        "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684",
                        "_id" : ObjectId("625df43c74679c25b6c157eb"),
                        "SharedInfoId" : "625da02c2752994b203d3681"
                    },
                    "TotDownvotes" : NumberLong(0),
                    "TotUpvotes" : NumberLong(0),
                    "VideoManifests" : [
                        {
                            "_m" : "1ca89e6c-716c-4936-b7dc-908c057a3e41",
                            "_id" : ObjectId("625df43c74679c25b6c157ed")
                        }
                    ]
                }
                """;

            DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.Videos);
            DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.VideoManifests);
            DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.Users);

            var modelMapSerializer = new ModelMapSerializer<Video>(dbContext.Engine);
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext);

            using var documentReader = new JsonReader(sourceDocument);
            var video = modelMapSerializer.Deserialize(BsonDeserializationContext.CreateRoot(documentReader));

            // Action.
            var serializedDocument = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(serializedDocument))
                modelMapSerializer.Serialize(
                    BsonSerializationContext.CreateRoot(bsonWriter),
                    new BsonSerializationArgs { NominalType = typeof(Video) },
                    video);

            // Assert.
            //the secondary schema read the element the summary was stored with: the reference is
            //written back whole, instead of the default value of a member nothing filled
            var manifestPreview = serializedDocument["LastValidManifest"].AsBsonDocument;
            Assert.Equal(
                "e15d54efa42840e74fde05137450f42e8c41f51f7541b1a6999eaa51fde04408",
                manifestPreview["ManifestReference"].AsString);
            Assert.True(manifestPreview["IsValid"].AsBoolean);
            Assert.Equal("8c4b7808-bc93-47c5-a4f3-68c1ce666311", manifestPreview["_s"].AsString);
            //the element the summary was stored with doesn't survive the rewrite
            Assert.False(manifestPreview.Contains("ManifestHash"));
        }
    }
}
