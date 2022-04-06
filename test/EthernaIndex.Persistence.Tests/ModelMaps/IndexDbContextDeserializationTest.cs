using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.ExecContext.AsyncLocal;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Options;
using Etherna.MongODM.Core.ProxyModels;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization.Mapping;
using Etherna.MongODM.Core.Serialization.Modifiers;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.MongODM.Core.Utility;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    public class IndexDbContextDeserializationTest
    {
        // Fields.
        private readonly IndexDbContext dbContext;

        // Constructor.
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Need to keep objects after test construction")]
        public IndexDbContextDeserializationTest()
        {
            // Setup dbContext.
            var eventDispatcherMock = new Mock<IEventDispatcher>();
            dbContext = new IndexDbContext(eventDispatcherMock.Object);

            // Setup dbcontext dependencies for initialization.
            Mock<IDbDependencies> dbDependenciesMock = new();
            var execContext = AsyncLocalContext.Instance;

            dbDependenciesMock.Setup(d => d.BsonSerializerRegistry).Returns(new BsonSerializerRegistry());
            dbDependenciesMock.Setup(d => d.DbCache).Returns(new DbCache());
            dbDependenciesMock.Setup(d => d.DbMaintainer).Returns(new Mock<IDbMaintainer>().Object);
            dbDependenciesMock.Setup(d => d.DbMigrationManager).Returns(new Mock<IDbMigrationManager>().Object);
            dbDependenciesMock.Setup(d => d.DiscriminatorRegistry).Returns(new DiscriminatorRegistry());
            dbDependenciesMock.Setup(d => d.ExecutionContext).Returns(execContext);
            dbDependenciesMock.Setup(d => d.ProxyGenerator).Returns(new ProxyGenerator(new Castle.DynamicProxy.ProxyGenerator()));
            dbDependenciesMock.Setup(d => d.RepositoryRegistry).Returns(new RepositoryRegistry());
            dbDependenciesMock.Setup(d => d.SchemaRegistry).Returns(new SchemaRegistry());
            dbDependenciesMock.Setup(d => d.SerializerModifierAccessor).Returns(new SerializerModifierAccessor(execContext));

            // Initialize dbContext.
            dbContext.Initialize(
                dbDependenciesMock.Object,
                new DbContextOptions(),
                Array.Empty<IDbContext>());
        }

        // Data.
        public static IEnumerable<object[]> CommentDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, Comment expectedComment)>();

                // "8e509e8e-5c2b-4874-a734-ada4e2b91f92" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""621d377079200245673f1071""), 
                            ""_m"" : ""8e509e8e-5c2b-4874-a734-ada4e2b91f92"", 
                            ""CreationDateTime"" : ISODate(""2022-02-28T20:58:24.825+0000""), 
                            ""Author"" : {
                                ""_m"" : ""caa0968f-4493-485b-b8d0-bc40942e8684"", 
                                ""_id"" : ObjectId(""6217ce1f89618c1a512354a1""), 
                                ""IdentityManifest"" : {
                                    ""Hash"" : ""e61f2a29a228b7f6374268b44b51cfa533ca42c2b14fffd47c2dc6ce123456f3""
                                }, 
                                ""SharedInfoId"" : ""61cdffb4fa7c4052d258123b""
                            }, 
                            ""Text"" : ""test"", 
                            ""Video"" : {
                                ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                ""_id"" : ObjectId(""621caf06ce0a123b360e640a"")
                            }
                        }";

                    var expectedCommentMock = new Mock<Comment>();
                    expectedCommentMock.Setup(c => c.Id).Returns("621d377079200245673f1071");
                    expectedCommentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2022, 02, 28, 20, 58, 24, 825));
                    {
                        var authorMock = new Mock<User>();
                        authorMock.Setup(a => a.Id).Returns("6217ce1f89618c1a512354a1");
                        expectedCommentMock.Setup(c => c.Author).Returns(authorMock.Object);
                    }
                    expectedCommentMock.Setup(c => c.Text).Returns("test");
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("621caf06ce0a123b360e640a");
                        expectedCommentMock.Setup(c => c.Video).Returns(videoMock.Object);
                    }

                    tests.Add((sourceDocument, expectedCommentMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedComment });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(CommentDeserializationTests))]
        public void CommentDeserialization(string sourceDocument, Comment expectedComment)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedComment is null)
                throw new ArgumentNullException(nameof(expectedComment));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Comment>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedComment.Id, result.Id);
            Assert.Equal(expectedComment.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedComment.Author.Id, result.Author.Id);
            Assert.Equal(expectedComment.Text, result.Text);
            Assert.Equal(expectedComment.Video.Id, result.Video.Id);
        }
    }
}
