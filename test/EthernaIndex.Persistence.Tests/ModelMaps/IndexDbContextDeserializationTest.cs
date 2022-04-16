using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Persistence.Helpers;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.MongODM.Core.Utility;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    public class IndexDbContextDeserializationTest
    {
        // Fields.
        private readonly IndexDbContext dbContext;
        private readonly Mock<IMongoDatabase> mongoDatabaseMock = new();

        // Constructor.
        public IndexDbContextDeserializationTest()
        {
            // Setup dbContext.
            var eventDispatcherMock = new Mock<IEventDispatcher>();
            dbContext = new IndexDbContext(eventDispatcherMock.Object);

            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);
        }

        // Data.
        public static IEnumerable<object[]> CommentDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<Comment>>();

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

                    tests.Add(new DeserializationTestElement<Comment>(sourceDocument, expectedCommentMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object []> ManualVideoReviewDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<ManualVideoReview>>();

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> UnsuitableVideoReportDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<UnsuitableVideoReport>>();

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

                    var setupAction = new Action<Mock<IMongoDatabase>, IIndexDbContext>((mongoDatabaseMock, dbContext) =>
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("621d54ab0a7a47231123c78f");

                        var videoCollectionMock = DbContextMockHelper.SetupCollectionMock(mongoDatabaseMock, dbContext.Videos);
                        DbContextMockHelper.SetupFindWithPredicate(videoCollectionMock, _ => new[] { videoMock.Object });
                    });

                    tests.Add(new DeserializationTestElement<UnsuitableVideoReport>(sourceDocument, expectedReportMock.Object, setupAction));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> UserDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<User>>();

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

                    tests.Add(new DeserializationTestElement<User>(sourceDocument, expectedUserMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> VideoDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<Video>>();

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

                    tests.Add(new DeserializationTestElement<Video>(sourceDocument, expectedVideoMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> VideoManifestDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<VideoManifest>>();

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
                        { "1920w", "5d2a835a77269dc7bb1fb6be7b12407326cf6dcde4bd14f41b92be9d82414421" },
                        { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008" },
                        { "960w", "60f8f4b17cdae08da8d03f7fa3476f47d7d29517351ffe7bd9f171b929680009" },
                        { "1440w", "7eb77f7d0c2d17d9e05036f154b4d26091ba3e7d0ccfe8ebda49cda2bb94cd9b" }
                    }));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> VideoVoteDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<VideoVote>>();

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

                    tests.Add(new DeserializationTestElement<VideoVote>(sourceDocument, expectedVoteMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(CommentDeserializationTests))]
        public void CommentDeserialization(DeserializationTestElement<Comment> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Comment>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.Author, result.Author, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Text, result.Text);
            Assert.Equal(testElement.ExpectedModel.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Author);
            Assert.NotNull(result.Text);
            Assert.NotNull(result.Video);
        }

        //[Theory, MemberData(nameof(ManualVideoReviewDeserializationTests))]
        //public void ManualVideoReviewDeserialization(DeserializationTestElement<ManualVideoReview> testElement)
        //{
        //    if (testElement is null)
        //        throw new ArgumentNullException(nameof(testElement));

        //    // Setup.
        //    using var documentReader = new JsonReader(testElement.SourceDocument);
        //    var modelMapSerializer = new ModelMapSerializer<ManualVideoReview>(dbContext);
        //    var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
        //    testElement.SetupAction(mongoDatabaseMock, dbContext);

        //    // Action.
        //    using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
        //    var result = modelMapSerializer.Deserialize(deserializationContext);

        //    // Assert.
        //    Assert.Equal(testElement.ExpectedModel.Id, result.Id);
        //    Assert.Equal(testElement.ExpectedModel.Author, result.Author, EntityModelEqualityComparer.Instance);
        //    Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
        //    Assert.Equal(testElement.ExpectedModel.Description, result.Description);
        //    Assert.Equal(testElement.ExpectedModel.IsValid, result.IsValid);
        //    Assert.Equal(testElement.ExpectedModel.Video, result.Video, EntityModelEqualityComparer.Instance);
        //    Assert.NotNull(result.Id);
        //    Assert.NotNull(result.Author);
        //    Assert.NotNull(result.Description);
        //    Assert.NotNull(result.Video);
        //}

        [Theory, MemberData(nameof(UnsuitableVideoReportDeserializationTests))]
        public void UnsuitableVideoReportDeserialization(DeserializationTestElement<UnsuitableVideoReport> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<UnsuitableVideoReport>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Description, result.Description);
            Assert.Equal(testElement.ExpectedModel.IsArchived, result.IsArchived);
            Assert.Equal(testElement.ExpectedModel.LastUpdate, result.LastUpdate);
            Assert.Equal(testElement.ExpectedModel.ReporterAuthor, result.ReporterAuthor, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.VideoManifest, result.VideoManifest, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Description);
            Assert.NotNull(result.LastUpdate);
            Assert.NotNull(result.ReporterAuthor);
            Assert.NotNull(result.Video);
            Assert.NotNull(result.VideoManifest);
        }

        [Theory, MemberData(nameof(UserDeserializationTests))]
        public void UserDeserialization(DeserializationTestElement<User> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<User>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.SharedInfoId, result.SharedInfoId);
            Assert.Equal(testElement.ExpectedModel.Videos, result.Videos, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.SharedInfoId);
            Assert.NotNull(result.Videos);
        }

        [Theory, MemberData(nameof(VideoDeserializationTests))]
        public void VideoDeserialization(DeserializationTestElement<Video> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<Video>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.IsFrozen, result.IsFrozen);
            Assert.Equal(testElement.ExpectedModel.Owner, result.Owner, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.TotDownvotes, result.TotDownvotes);
            Assert.Equal(testElement.ExpectedModel.TotUpvotes, result.TotUpvotes);
            Assert.Equal(testElement.ExpectedModel.VideoManifests, result.VideoManifests, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Owner);
            Assert.NotNull(result.VideoManifests);
        }

        [Theory, MemberData(nameof(VideoManifestDeserializationTests))]
        public void VideoManifestDeserialization(DeserializationTestElement<VideoManifest> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<VideoManifest>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Description, result.Description);
            Assert.Equal(testElement.ExpectedModel.Duration, result.Duration);
            Assert.Equal(testElement.ExpectedModel.ErrorValidationResults, result.ErrorValidationResults);
            Assert.Equal(testElement.ExpectedModel.IsValid, result.IsValid);
            Assert.Equal(testElement.ExpectedModel.Manifest, result.Manifest);
            Assert.Equal(testElement.ExpectedModel.OriginalQuality, result.OriginalQuality);
            Assert.Equal(testElement.ExpectedModel.Sources, result.Sources);
            Assert.Equal(testElement.ExpectedModel.Thumbnail, result.Thumbnail);
            Assert.Equal(testElement.ExpectedModel.Title, result.Title);
            Assert.Equal(testElement.ExpectedModel.ValidationTime, result.ValidationTime);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Description);
            Assert.NotNull(result.Duration);
            Assert.NotNull(result.ErrorValidationResults);
            Assert.NotNull(result.IsValid);
            Assert.NotNull(result.Manifest);
            Assert.NotNull(result.OriginalQuality);
            Assert.NotNull(result.Sources);
            Assert.NotNull(result.Thumbnail);
            Assert.NotNull(result.Title);
            Assert.NotNull(result.ValidationTime);
        }

        [Theory, MemberData(nameof(VideoVoteDeserializationTests))]
        public void VideoVoteDeserialization(DeserializationTestElement<VideoVote> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<VideoVote>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Owner, result.Owner, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.Value, result.Value);
            Assert.Equal(testElement.ExpectedModel.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Owner);
            Assert.NotNull(result.Video);
        }
    }
}
