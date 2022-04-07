using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.ManifestAgg;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.EthernaIndex.Persistence.Helpers;
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

        public static IEnumerable<object []> ManualVideoReviewDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, ManualVideoReview expectedReview)>();

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedReview });
            }
        }

        public static IEnumerable<object[]> UnsuitableVideoReportDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, UnsuitableVideoReport expectedReport)>();

                // "91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""621e32b14075df3daa1a34d5""), 
                            ""_m"" : ""91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d"", 
                            ""CreationDateTime"" : ISODate(""2022-03-01T14:50:25.134+0000""), 
                            ""Description"" : ""violence"", 
                            ""LastUpdate"" : ISODate(""2022-04-01T14:50:25.134+0000""), 
                            ""ReporterAuthor"" : {
                                ""_m"" : ""caa0968f-4493-485b-b8d0-bc40942e8684"", 
                                ""_id"" : ObjectId(""621d38a179200252573a008e""), 
                                ""IdentityManifest"" : {
                                    ""Hash"" : ""07ca616dfd12337455c386f377ee4647a99d6550af3033f9b5a12a9ed5262cf0""
                                }, 
                                ""SharedInfoId"" : ""62189f757a067d123b7c4ec3""
                            }, 
                            ""VideoManifest"" : {
                                ""_m"" : ""f7966611-14aa-4f18-92f4-8697b4927fb6"", 
                                ""CreationDateTime"" : ISODate(""2022-02-28T23:03:07.658+0000""), 
                                ""_id"" : ObjectId(""621d54ab0a7a47231123c790""), 
                                ""IsValid"" : true, 
                                ""Manifest"" : {
                                    ""Hash"" : ""653443644f0a0d3ed874dae2e1735df91237390bfe87096e437f28322f957d41""
                                }, 
                                ""Video"" : {
                                    ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                    ""_id"" : ObjectId(""621d54ab0a7a47231123c78f"")
                                }, 
                                ""Title"" : ""test""
                            }
                        }";

                    var expectedReportMock = new Mock<UnsuitableVideoReport>();
                    expectedReportMock.Setup(r => r.Id).Returns("621e32b14075df3daa1a34d5");
                    expectedReportMock.Setup(r => r.CreationDateTime).Returns(new DateTime(2022, 03, 01, 14, 50, 25, 134));
                    expectedReportMock.Setup(r => r.Description).Returns("violence");
                    expectedReportMock.Setup(r => r.LastUpdate).Returns(new DateTime(2022, 04, 01, 14, 50, 25, 134));
                    {
                        var authorMock = new Mock<User>();
                        authorMock.Setup(a => a.Id).Returns("621d38a179200252573a008e");
                        expectedReportMock.Setup(c => c.ReporterAuthor).Returns(authorMock.Object);
                    }
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("621d54ab0a7a47231123c78f");
                        expectedReportMock.Setup(c => c.Video).Returns(videoMock.Object);
                    }
                    {
                        var videoManifestMock = new Mock<VideoManifest>();
                        videoManifestMock.Setup(v => v.Id).Returns("621d54ab0a7a47231123c790");
                        expectedReportMock.Setup(c => c.VideoManifest).Returns(videoManifestMock.Object);
                    }

                    tests.Add((sourceDocument, expectedReportMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedReport });
            }
        }

        public static IEnumerable<object[]> UserDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, User expectedUser)>();

                // "a547abdc-420c-41f9-b496-e6cf704a3844" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""6217ce3489618456527854e4""), 
                            ""_m"" : ""a547abdc-420c-41f9-b496-e6cf704a3844"", 
                            ""CreationDateTime"" : ISODate(""2022-02-24T18:28:04.685+0000""), 
                            ""IdentityManifest"" : {
                                ""Hash"" : ""581e7f32c667eedf974566f52646a04cce04987a735a8af44c225f2ad085508e""
                            }, 
                            ""SharedInfoId"" : ""61cdeb616b35d3455b9d68ce"", 
                            ""Videos"" : [
                                {
                                    ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                    ""_id"" : ObjectId(""6229f4e50a7a47231567c7af"")
                                }, 
                                {
                                    ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                    ""_id"" : ObjectId(""6233d1a2340695c8e564391a"")
                                }
                            ]
                        }";

                    var expectedUserMock = new Mock<User>();
                    expectedUserMock.Setup(u => u.Id).Returns("6217ce3489618456527854e4");
                    expectedUserMock.Setup(u => u.CreationDateTime).Returns(new DateTime(2022, 02, 24, 18, 28, 04, 685));
                    expectedUserMock.Setup(u => u.SharedInfoId).Returns("61cdeb616b35d3455b9d68ce");
                    {
                        var video0Mock = new Mock<Video>();
                        var video1Mock = new Mock<Video>();
                        video0Mock.Setup(a => a.Id).Returns("6229f4e50a7a47231567c7af");
                        video1Mock.Setup(a => a.Id).Returns("6233d1a2340695c8e564391a");
                        expectedUserMock.Setup(c => c.Videos).Returns(new[] { video0Mock.Object, video1Mock.Object });
                    }

                    tests.Add((sourceDocument, expectedUserMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedUser });
            }
        }

        public static IEnumerable<object[]> VideoDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, Video expectedVideo)>();

                // "abfbd104-35ff-4429-9afc-79304a11efc0" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""6229f4e50a4567231a0ec7af""), 
                            ""_m"" : ""abfbd104-35ff-4429-9afc-79304a11efc0"", 
                            ""CreationDateTime"" : ISODate(""2022-03-10T12:53:57.191+0000""), 
                            ""EncryptionKey"" : null, 
                            ""EncryptionType"" : ""Plain"", 
                            ""Owner"" : {
                                ""_m"" : ""caa0968f-4493-485b-b8d0-bc40942e8684"", 
                                ""_id"" : ObjectId(""6217ce348967891a527854e4""), 
                                ""IdentityManifest"" : {
                                    ""Hash"" : ""581e7f32c667eedf975745652646a04cce04987a735a8af44c225f2ad085508e""
                                }, 
                                ""SharedInfoId"" : ""61cdeb611235d8985b9d68ce""
                            }, 
                            ""TotDownvotes"" : NumberLong(1), 
                            ""TotUpvotes"" : NumberLong(2), 
                            ""VideoManifests"" : [
                                {
                                    ""_m"" : ""f7966611-14aa-4f18-92f4-8697b4927fb6"", 
                                    ""CreationDateTime"" : ISODate(""2022-03-10T12:53:57.235+0000""), 
                                    ""_id"" : ObjectId(""6229f475127a47231a0ec7b0""), 
                                    ""IsValid"" : true, 
                                    ""Manifest"" : {
                                        ""Hash"" : ""8d14d87c6663d39fb5e57ae46963588c61eb0c9641dfac23cb3c37ec189d2634""
                                    }, 
                                    ""Video"" : {
                                        ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                        ""_id"" : ObjectId(""6229f4e50a7a47231a7537af"")
                                    }, 
                                    ""Title"" : ""Test1""
                                }, 
                                {
                                    ""_m"" : ""f7966611-14aa-4f18-92f4-8697b4927fb6"", 
                                    ""CreationDateTime"" : ISODate(""2022-03-10T13:17:57.667+0000""), 
                                    ""_id"" : ObjectId(""6229fa8540452f3d336a34ee""), 
                                    ""IsValid"" : true, 
                                    ""Manifest"" : {
                                        ""Hash"" : ""3ef2e441eac00e3685615bf90a16385b7aee97084f4cab6301bdbf76a1ed9d74""
                                    }, 
                                    ""Video"" : {
                                        ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                        ""_id"" : ObjectId(""6229f4e50a7a47231a7537af"")
                                    }, 
                                    ""Title"" : ""Test2""
                                }
                            ]
                        }";

                    var expectedVideoMock = new Mock<Video>();
                    expectedVideoMock.Setup(v => v.Id).Returns("6229f4e50a4567231a0ec7af");
                    expectedVideoMock.Setup(v => v.CreationDateTime).Returns(new DateTime(2022, 03, 10, 12, 53, 57, 191));
                    expectedVideoMock.Setup(v => v.IsFrozen).Returns(false);
                    {
                        var ownerMock = new Mock<User>();
                        ownerMock.Setup(u => u.Id).Returns("6217ce348967891a527854e4");
                        expectedVideoMock.Setup(v => v.Owner).Returns(ownerMock.Object);
                    }
                    expectedVideoMock.Setup(v => v.TotDownvotes).Returns(1);
                    expectedVideoMock.Setup(v => v.TotUpvotes).Returns(2);
                    {
                        var manifest0Mock = new Mock<VideoManifest>();
                        var manifest1Mock = new Mock<VideoManifest>();
                        manifest0Mock.Setup(m => m.Id).Returns("6229f475127a47231a0ec7b0");
                        manifest1Mock.Setup(m => m.Id).Returns("6229fa8540452f3d336a34ee");
                        expectedVideoMock.Setup(v => v.VideoManifests).Returns(new[] { manifest0Mock.Object, manifest1Mock.Object });
                    }

                    tests.Add((sourceDocument, expectedVideoMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedVideo });
            }
        }

        public static IEnumerable<object[]> VideoManifestDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, VideoManifest expectedVideoManifest)>();

                // "ec578080-ccd2-4d49-8a76-555b10a5dad5" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""622e619a0a7a47231a0ec7b5""), 
                            ""_m"" : ""ec578080-ccd2-4d49-8a76-555b10a5dad5"", 
                            ""CreationDateTime"" : ISODate(""2022-03-13T21:26:50.359+0000""), 
                            ""ErrorValidationResults"" : [

                            ], 
                            ""IsValid"" : true, 
                            ""Manifest"" : {
                                ""Hash"" : ""ce601b421535419ae5c536d736075afb9eaac39e304c75357ef9312251704232""
                            }, 
                            ""ValidationTime"" : ISODate(""2022-03-13T21:26:50.455+0000""), 
                            ""Description"" : ""Test description"", 
                            ""Duration"" : 900.0054321289062, 
                            ""OriginalQuality"" : ""720p"", 
                            ""Sources"" : [
                                {
                                    ""Bitrate"" : NumberInt(557647), 
                                    ""Quality"" : ""720p"", 
                                    ""Reference"" : ""d88f68aa5b157ce6bda355d8bd54179df264a899c03bf5bdf0d4569f20a6933b"", 
                                    ""Size"" : NumberInt(62735710)
                                }
                            ], 
                            ""Title"" : ""Etherna WAM presentation"", 
                            ""Thumbnail"" : {
                                ""AspectRatio"" : 1.7777777910232544, 
                                ""BlurHash"" : ""LEHV6nWB2yk8pyo0adR*.7kCMdnj"", 
                                ""Sources"" : {
                                    ""1920w"" : ""5d2a835a77269dc7bb1fb6be7b12407326cf6dcde4bd14f41b92be9d82414421"", 
                                    ""480w"" : ""a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"", 
                                    ""960w"" : ""60f8f4b17cdae08da8d03f7fa3476f47d7d29517351ffe7bd9f171b929680009"", 
                                    ""1440w"" : ""7eb77f7d0c2d17d9e05036f154b4d26091ba3e7d0ccfe8ebda49cda2bb94cd9b""
                                }
                            }, 
                            ""Video"" : {
                                ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                ""_id"" : ObjectId(""6229f4e50a7a47231a0ec7af"")
                            }
                        }";

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("622e619a0a7a47231a0ec7b5");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 03, 13, 21, 26, 50, 359));
                    expectedManifestMock.Setup(m => m.ErrorValidationResults).Returns(Array.Empty<ErrorDetail>());
                    expectedManifestMock.Setup(m => m.IsValid).Returns(true);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("ce601b421535419ae5c536d736075afb9eaac39e304c75357ef9312251704232"));
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 03, 13, 21, 26, 50, 455));
                    expectedManifestMock.Setup(m => m.Description).Returns("Test description");
                    expectedManifestMock.Setup(m => m.Duration).Returns(900.0054321289062f);
                    expectedManifestMock.Setup(m => m.OriginalQuality).Returns("720p");
                    expectedManifestMock.Setup(m => m.Sources).Returns(new[]{
                        new VideoSource(557647, "720p", "d88f68aa5b157ce6bda355d8bd54179df264a899c03bf5bdf0d4569f20a6933b", 62735710)
                    });
                    expectedManifestMock.Setup(m => m.Title).Returns("Etherna WAM presentation");
                    expectedManifestMock.Setup(m => m.Thumbnail).Returns(new SwarmImageRaw(1.7777777910232544f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string>
                    {
                        { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008" },
                        { "960w", "60f8f4b17cdae08da8d03f7fa3476f47d7d29517351ffe7bd9f171b929680009" },
                        { "1440w", "7eb77f7d0c2d17d9e05036f154b4d26091ba3e7d0ccfe8ebda49cda2bb94cd9b" },
                        { "1920w", "5d2a835a77269dc7bb1fb6be7b12407326cf6dcde4bd14f41b92be9d82414421" }
                    }));

                    tests.Add((sourceDocument, expectedManifestMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedVideoManifest });
            }
        }

        public static IEnumerable<object[]> VideoVoteDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, VideoVote expectedVideoVote)>();

                // "624955bf-8c09-427f-93da-fc6ddb9668a6" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""621e90110a7a47231a0ec797""), 
                            ""_m"" : ""624955bf-8c09-427f-93da-fc6ddb9668a6"", 
                            ""CreationDateTime"" : ISODate(""2022-03-01T21:28:49.590+0000""), 
                            ""Owner"" : {
                                ""_m"" : ""caa0968f-4493-485b-b8d0-bc40942e8684"", 
                                ""_id"" : ObjectId(""621d38a179200252573f108e""), 
                                ""IdentityManifest"" : {
                                    ""Hash"" : ""07ca616dfd2f137455c386f377ee4647a99d6550af3033f9b5a12a9ed5262cf0""
                                }, 
                                ""SharedInfoId"" : ""62189f757a067d558b7c4ec3""
                            }, 
                            ""Value"" : ""Up"", 
                            ""Video"" : {
                                ""_m"" : ""d4844740-472d-48b9-b066-67ba9a2acc9b"", 
                                ""_id"" : ObjectId(""621d54ab0a7a47231a0ec78f"")
                            }
                        }";

                    var expectedVoteMock = new Mock<VideoVote>();
                    expectedVoteMock.Setup(m => m.Id).Returns("621e90110a7a47231a0ec797");
                    expectedVoteMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 03, 01, 21, 28, 49, 590));
                    {
                        var ownerMock = new Mock<User>();
                        ownerMock.Setup(o => o.Id).Returns("621d38a179200252573f108e");
                        expectedVoteMock.Setup(m => m.Owner).Returns(ownerMock.Object);
                    }
                    expectedVoteMock.Setup(v => v.Value).Returns(VoteValue.Up);
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("621d54ab0a7a47231a0ec78f");
                        expectedVoteMock.Setup(v => v.Video).Returns(videoMock.Object);
                    }

                    tests.Add((sourceDocument, expectedVoteMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedVideoVote });
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
            Assert.Equal(expectedComment.Author, result.Author, EntityModelEqualityComparer.Instance);
            Assert.Equal(expectedComment.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedComment.Text, result.Text);
            Assert.Equal(expectedComment.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(expectedComment.Id);
            Assert.NotNull(expectedComment.Author);
            Assert.NotNull(expectedComment.Text);
            Assert.NotNull(expectedComment.Video);
        }

        [Theory, MemberData(nameof(ManualVideoReviewDeserializationTests))]
        public void ManualVideoReviewDeserialization(string sourceDocument, ManualVideoReview expectedReview)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedReview is null)
                throw new ArgumentNullException(nameof(expectedReview));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<ManualVideoReview>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedReview.Id, result.Id);
            Assert.Equal(expectedReview.Author, result.Author, EntityModelEqualityComparer.Instance);
            Assert.Equal(expectedReview.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedReview.Description, result.Description);
            Assert.Equal(expectedReview.IsValid, result.IsValid);
            Assert.Equal(expectedReview.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(expectedReview.Id);
            Assert.NotNull(expectedReview.Author);
            Assert.NotNull(expectedReview.Description);
            Assert.NotNull(expectedReview.Video);
        }

        [Theory, MemberData(nameof(UnsuitableVideoReportDeserializationTests))]
        public void UnsuitableVideoReportDeserialization(string sourceDocument, UnsuitableVideoReport expectedReport)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedReport is null)
                throw new ArgumentNullException(nameof(expectedReport));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<UnsuitableVideoReport>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedReport.Id, result.Id);
            Assert.Equal(expectedReport.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedReport.Description, result.Description);
            Assert.Equal(expectedReport.IsArchived, result.IsArchived);
            Assert.Equal(expectedReport.LastUpdate, result.LastUpdate);
            Assert.Equal(expectedReport.ReporterAuthor, result.ReporterAuthor, EntityModelEqualityComparer.Instance);
            Assert.Equal(expectedReport.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.Equal(expectedReport.VideoManifest, result.VideoManifest, EntityModelEqualityComparer.Instance);
            Assert.NotNull(expectedReport.Id);
            Assert.NotNull(expectedReport.Description);
            Assert.NotNull(expectedReport.LastUpdate);
            Assert.NotNull(expectedReport.ReporterAuthor);
            Assert.NotNull(expectedReport.Video);
            Assert.NotNull(expectedReport.VideoManifest);
        }

        [Theory, MemberData(nameof(UserDeserializationTests))]
        public void UserDeserialization(string sourceDocument, User expectedUser)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedUser is null)
                throw new ArgumentNullException(nameof(expectedUser));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<User>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedUser.Id, result.Id);
            Assert.Equal(expectedUser.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedUser.SharedInfoId, result.SharedInfoId);
            Assert.Equal(expectedUser.Videos, result.Videos, EntityModelEqualityComparer.Instance);
            Assert.NotNull(expectedUser.Id);
            Assert.NotNull(expectedUser.SharedInfoId);
            Assert.NotNull(expectedUser.Videos);
        }

        [Theory, MemberData(nameof(VideoDeserializationTests))]
        public void VideoDeserialization(string sourceDocument, Video expectedVideo)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedVideo is null)
                throw new ArgumentNullException(nameof(expectedVideo));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Video>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedVideo.Id, result.Id);
            Assert.Equal(expectedVideo.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedVideo.IsFrozen, result.IsFrozen);
            Assert.Equal(expectedVideo.Owner, result.Owner, EntityModelEqualityComparer.Instance);
            Assert.Equal(expectedVideo.TotDownvotes, result.TotDownvotes);
            Assert.Equal(expectedVideo.TotUpvotes, result.TotUpvotes);
            Assert.Equal(expectedVideo.VideoManifests, result.VideoManifests, EntityModelEqualityComparer.Instance);
            Assert.NotNull(expectedVideo.Id);
            Assert.NotNull(expectedVideo.Owner);
            Assert.NotNull(expectedVideo.VideoManifests);
        }

        [Theory, MemberData(nameof(VideoManifestDeserializationTests))]
        public void VideoManifestDeserialization(string sourceDocument, VideoManifest expectedVideoManifest)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedVideoManifest is null)
                throw new ArgumentNullException(nameof(expectedVideoManifest));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<VideoManifest>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedVideoManifest.Id, result.Id);
            Assert.Equal(expectedVideoManifest.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedVideoManifest.Description, result.Description);
            Assert.Equal(expectedVideoManifest.Duration, result.Duration);
            Assert.Equal(expectedVideoManifest.ErrorValidationResults, result.ErrorValidationResults); //maybe need to fix, because it isn't an entity
            Assert.Equal(expectedVideoManifest.IsValid, result.IsValid);
            Assert.Equal(expectedVideoManifest.Manifest, result.Manifest);
            Assert.Equal(expectedVideoManifest.OriginalQuality, result.OriginalQuality);
            Assert.Equal(expectedVideoManifest.Sources, result.Sources);
            Assert.Equal(expectedVideoManifest.Thumbnail, result.Thumbnail);
            Assert.Equal(expectedVideoManifest.Title, result.Title);
            Assert.Equal(expectedVideoManifest.ValidationTime, result.ValidationTime);
            Assert.NotNull(expectedVideoManifest.Id);
            Assert.NotNull(expectedVideoManifest.Description);
            Assert.NotNull(expectedVideoManifest.Duration);
            Assert.NotNull(expectedVideoManifest.ErrorValidationResults);
            Assert.NotNull(expectedVideoManifest.IsValid);
            Assert.NotNull(expectedVideoManifest.Manifest);
            Assert.NotNull(expectedVideoManifest.OriginalQuality);
            Assert.NotNull(expectedVideoManifest.Sources);
            Assert.NotNull(expectedVideoManifest.Thumbnail);
            Assert.NotNull(expectedVideoManifest.Title);
            Assert.NotNull(expectedVideoManifest.ValidationTime);
        }

        [Theory, MemberData(nameof(VideoVoteDeserializationTests))]
        public void VideoVoteDeserialization(string sourceDocument, VideoVote expectedVideoVote)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedVideoVote is null)
                throw new ArgumentNullException(nameof(expectedVideoVote));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<VideoVote>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedVideoVote.Id, result.Id);
            Assert.Equal(expectedVideoVote.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedVideoVote.Owner, result.Owner, EntityModelEqualityComparer.Instance);
            Assert.Equal(expectedVideoVote.Value, result.Value);
            Assert.Equal(expectedVideoVote.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(expectedVideoVote.Id);
            Assert.NotNull(expectedVideoVote.Owner);
            Assert.NotNull(expectedVideoVote.Video);
        }
    }
}
