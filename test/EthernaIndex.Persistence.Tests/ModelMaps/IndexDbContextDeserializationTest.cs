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

using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Persistence.Helpers;
using Etherna.MongoDB.Bson.IO;
using Etherna.MongoDB.Bson.Serialization;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Serialization.Serializers;
using Etherna.MongODM.Core.Utility;
using Microsoft.Extensions.Logging;
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
            var loggerMock = new Mock<ILogger<IndexDbContext>>();
            dbContext = new IndexDbContext(eventDispatcherMock.Object, loggerMock.Object);

            DbContextMockHelper.InitializeDbContextMock(dbContext, mongoDatabaseMock);
        }

        // Data.
        public static IEnumerable<object[]> CommentDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<Comment>>();
                
                // "a846e95a-f99b-4d66-91a8-807a1ef34140" - v0.3.10
                {
                    var sourceDocument =
                        $$"""
                        {
                            "_id" : ObjectId("621d377079200245673f1071"),
                            "_m" : "a846e95a-f99b-4d66-91a8-807a1ef34140",
                            "CreationDateTime" : ISODate("2023-09-07T22:15:48.183+0000"),
                            "Author" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684",
                                "_id" : ObjectId("6217ce1f89618c1a512354a1"),
                                "SharedInfoId" : "61cdffb4fa7c4052d258123b"
                            },
                            "IsFrozen" : true,
                            "TextHistory" : [
                                {
                                    "k" : ISODate("2023-09-07T22:15:48.183+0000"),
                                    "v" : "This is a comment"
                                },
                                {
                                    "k" : ISODate("2023-09-07T22:17:08.438+0000"),
                                    "v" : "And this is an edit"
                                }
                            ],
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b",
                                "_id" : ObjectId("621caf06ce0a123b360e640a")
                            }
                        }
                        """;

                    var expectedCommentMock = new Mock<Comment>();
                    expectedCommentMock.Setup(c => c.Id).Returns("621d377079200245673f1071");
                    expectedCommentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2023, 09, 07, 22, 15, 48, 183));
                    {
                        var authorMock = new Mock<User>();
                        authorMock.Setup(a => a.Id).Returns("6217ce1f89618c1a512354a1");
                        expectedCommentMock.Setup(c => c.Author).Returns(authorMock.Object);
                    }
                    expectedCommentMock.Setup(c => c.IsFrozen).Returns(true);
                    expectedCommentMock.Setup(c => c.TextHistory).Returns(
                        new Dictionary<DateTime, string>
                        {
                            [new DateTime(2023, 09, 07, 22, 15, 48, 183)] = "This is a comment",
                            [new DateTime(2023, 09, 07, 22, 17, 08, 438)] = "And this is an edit"
                        });
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("621caf06ce0a123b360e640a");
                        expectedCommentMock.Setup(c => c.Video).Returns(videoMock.Object);
                    }

                    tests.Add(new DeserializationTestElement<Comment>(sourceDocument, expectedCommentMock.Object));
                }

                // "8e509e8e-5c2b-4874-a734-ada4e2b91f92" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("621d377079200245673f1071"), 
                            "_m" : "8e509e8e-5c2b-4874-a734-ada4e2b91f92", 
                            "CreationDateTime" : ISODate("2022-02-28T20:58:24.825+0000"), 
                            "Author" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("6217ce1f89618c1a512354a1"), 
                                "IdentityManifest" : {
                                    "Hash" : "e61f2a29a228b7f6374268b44b51cfa533ca42c2b14fffd47c2dc6ce123456f3"
                                }, 
                                "SharedInfoId" : "61cdffb4fa7c4052d258123b"
                            }, 
                            "IsFrozen" : true,
                            "Text" : "test", 
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                "_id" : ObjectId("621caf06ce0a123b360e640a")
                            }
                        }
                        """;

                    var expectedCommentMock = new Mock<Comment>();
                    expectedCommentMock.Setup(c => c.Id).Returns("621d377079200245673f1071");
                    expectedCommentMock.Setup(c => c.CreationDateTime).Returns(new DateTime(2022, 02, 28, 20, 58, 24, 825));
                    {
                        var authorMock = new Mock<User>();
                        authorMock.Setup(a => a.Id).Returns("6217ce1f89618c1a512354a1");
                        expectedCommentMock.Setup(c => c.Author).Returns(authorMock.Object);
                    }
                    expectedCommentMock.Setup(c => c.IsFrozen).Returns(true);
                    expectedCommentMock.Setup(c => c.TextHistory).Returns(
                        new Dictionary<DateTime, string>
                        {
                            [new DateTime(2022, 02, 28, 20, 58, 24, 825)] = "test"
                        });
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

        public static IEnumerable<object[]> ManualVideoReviewDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<ManualVideoReview>>();

                // "e3e734ab-d845-4ec2-8920-68956eba950d" - v0.3.0
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("625e913775060536d8a75a8c"), 
                            "_m" : "e3e734ab-d845-4ec2-8920-68956eba950d", 
                            "CreationDateTime" : ISODate("2022-04-19T10:38:47.311+0000"), 
                            "Author" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("625df43c74679c25b6c157eb"), 
                                "SharedInfoId" : "625da02c2752994b203d3681"
                            }, 
                            "Description" : "Sample description", 
                            "IsValidResult" : true, 
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                "_id" : ObjectId("625df43c74679c25b6c157ec")
                            }
                        }
                        """;

                    var expectedReviewMock = new Mock<ManualVideoReview>();
                    expectedReviewMock.Setup(r => r.Id).Returns("625e913775060536d8a75a8c");
                    expectedReviewMock.Setup(r => r.CreationDateTime).Returns(new DateTime(2022, 04, 19, 10, 38, 47, 311));
                    {
                        var authorMock = new Mock<User>();
                        authorMock.Setup(a => a.Id).Returns("625df43c74679c25b6c157eb");
                        expectedReviewMock.Setup(c => c.Author).Returns(authorMock.Object);
                    }
                    expectedReviewMock.Setup(r => r.Description).Returns("Sample description");
                    expectedReviewMock.Setup(r => r.IsValidResult).Returns(true);
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("625df43c74679c25b6c157ec");
                        expectedReviewMock.Setup(c => c.Video).Returns(videoMock.Object);
                    }

                    tests.Add(new DeserializationTestElement<ManualVideoReview>(sourceDocument, expectedReviewMock.Object));
                }

                return tests.Select(t => new object[] { t });
            }
        }

        public static IEnumerable<object[]> UnsuitableVideoReportDeserializationTests
        {
            get
            {
                var tests = new List<DeserializationTestElement<UnsuitableVideoReport>>();

                // "39e398d3-3199-43e1-8147-2876b534fbec" - v0.3.0
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("625e910375060536d8a75a8b"), 
                            "_m" : "39e398d3-3199-43e1-8147-2876b534fbec", 
                            "CreationDateTime" : ISODate("2022-04-19T10:37:55.057+0000"), 
                            "Description" : "illegal content", 
                            "IsArchived" : true, 
                            "LastUpdate" : ISODate("2022-04-01T14:50:25.134+0000"), 
                            "ReporterAuthor" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("625df43c74679c25b6c157eb"), 
                                "SharedInfoId" : "625da02c2752994b203d3681"
                            }, 
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                "_id" : ObjectId("625df43c74679c25b6c157ec")
                            }, 
                            "VideoManifest" : {
                                "_m" : "1ca89e6c-716c-4936-b7dc-908c057a3e41", 
                                "_id" : ObjectId("625df43c74679c25b6c157ed")
                            }
                        }
                        """;

                    var expectedReportMock = new Mock<UnsuitableVideoReport>();
                    expectedReportMock.Setup(r => r.Id).Returns("625e910375060536d8a75a8b");
                    expectedReportMock.Setup(r => r.CreationDateTime).Returns(new DateTime(2022, 04, 19, 10, 37, 55, 057));
                    expectedReportMock.Setup(r => r.Description).Returns("illegal content");
                    expectedReportMock.Setup(r => r.IsArchived).Returns(true);
                    expectedReportMock.Setup(r => r.LastUpdate).Returns(new DateTime(2022, 04, 01, 14, 50, 25, 134));
                    {
                        var authorMock = new Mock<User>();
                        authorMock.Setup(a => a.Id).Returns("625df43c74679c25b6c157eb");
                        expectedReportMock.Setup(c => c.ReporterAuthor).Returns(authorMock.Object);
                    }
                    {
                        var videoMock = new Mock<Video>();
                        videoMock.Setup(v => v.Id).Returns("625df43c74679c25b6c157ec");
                        expectedReportMock.Setup(c => c.Video).Returns(videoMock.Object);
                    }
                    {
                        var videoManifestMock = new Mock<VideoManifest>();
                        videoManifestMock.Setup(v => v.Id).Returns("625df43c74679c25b6c157ed");
                        expectedReportMock.Setup(c => c.VideoManifest).Returns(videoManifestMock.Object);
                    }

                    tests.Add(new DeserializationTestElement<UnsuitableVideoReport>(sourceDocument, expectedReportMock.Object));
                }

                // "91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("621e32b14075df3daa1a34d5"), 
                            "_m" : "91e7a66a-d1e2-48eb-9627-3c3c2ceb5e2d", 
                            "CreationDateTime" : ISODate("2022-03-01T14:50:25.134+0000"), 
                            "Description" : "violence", 
                            "LastUpdate" : ISODate("2022-04-01T14:50:25.134+0000"), 
                            "ReporterAuthor" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("621d38a179200252573a008e"), 
                                "IdentityManifest" : {
                                    "Hash" : "07ca616dfd12337455c386f377ee4647a99d6550af3033f9b5a12a9ed5262cf0"
                                }, 
                                "SharedInfoId" : "62189f757a067d123b7c4ec3"
                            }, 
                            "VideoManifest" : {
                                "_m" : "f7966611-14aa-4f18-92f4-8697b4927fb6", 
                                "CreationDateTime" : ISODate("2022-02-28T23:03:07.658+0000"), 
                                "_id" : ObjectId("621d54ab0a7a47231123c790"), 
                                "IsValid" : true, 
                                "Manifest" : {
                                    "Hash" : "653443644f0a0d3ed874dae2e1735df91237390bfe87096e437f28322f957d41"
                                }, 
                                "Video" : {
                                    "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                    "_id" : ObjectId("621d54ab0a7a47231123c78f")
                                }, 
                                "Title" : "test"
                            }
                        }
                        """;

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

                // "9a2d9664-31d5-4394-9a20-c8789cf0600d" - v0.3.0
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("625df43c74679c25b6c157eb"), 
                            "_m" : "9a2d9664-31d5-4394-9a20-c8789cf0600d", 
                            "CreationDateTime" : ISODate("2022-04-18T23:29:00.718+0000"), 
                            "SharedInfoId" : "625da02c2752994b203d3681", 
                            "Videos" : [
                                {
                                    "_m" : "cd4517e3-809d-455c-b7da-ba07c9e7280f", 
                                    "_id" : ObjectId("625df43c74679c25b6c157ec"), 
                                    "LastValidManifest" : {
                                        "_m" : "f7966611-14aa-4f18-92f4-8697b4927fb6", 
                                        "CreationDateTime" : ISODate("2022-04-18T23:29:00.919+0000"), 
                                        "_id" : ObjectId("625df43c74679c25b6c157ed"), 
                                        "IsValid" : true, 
                                        "Manifest" : {
                                            "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8", 
                                            "Hash" : "568863d1a27feb3682b720d43cebd723ee09ce57c538831bf94bafc9408871c9"
                                        }, 
                                        "Duration" : 420.0, 
                                        "Thumbnail" : null, 
                                        "Title" : "Mocked sample video"
                                    }
                                }
                            ]
                        }
                        """;

                    var expectedUserMock = new Mock<User>();
                    expectedUserMock.Setup(u => u.Id).Returns("625df43c74679c25b6c157eb");
                    expectedUserMock.Setup(u => u.CreationDateTime).Returns(new DateTime(2022, 04, 18, 23, 29, 00, 718));
                    expectedUserMock.Setup(u => u.SharedInfoId).Returns("625da02c2752994b203d3681");

                    tests.Add(new DeserializationTestElement<User>(sourceDocument, expectedUserMock.Object));
                }

                // "a547abdc-420c-41f9-b496-e6cf704a3844" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("6217ce3489618456527854e4"), 
                            "_m" : "a547abdc-420c-41f9-b496-e6cf704a3844", 
                            "CreationDateTime" : ISODate("2022-02-24T18:28:04.685+0000"), 
                            "IdentityManifest" : {
                                "Hash" : "581e7f32c667eedf974566f52646a04cce04987a735a8af44c225f2ad085508e"
                            }, 
                            "SharedInfoId" : "61cdeb616b35d3455b9d68ce", 
                            "Videos" : [
                                {
                                    "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                    "_id" : ObjectId("6229f4e50a7a47231567c7af")
                                }, 
                                {
                                    "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                    "_id" : ObjectId("6233d1a2340695c8e564391a")
                                }
                            ]
                        }
                        """;

                    var expectedUserMock = new Mock<User>();
                    expectedUserMock.Setup(u => u.Id).Returns("6217ce3489618456527854e4");
                    expectedUserMock.Setup(u => u.CreationDateTime).Returns(new DateTime(2022, 02, 24, 18, 28, 04, 685));
                    expectedUserMock.Setup(u => u.SharedInfoId).Returns("61cdeb616b35d3455b9d68ce");

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

                // "d0c48dd8-0887-4ac5-80e5-9b08c5dc77f1" - v0.3.0
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("625df43c74679c25b6c157ec"), 
                            "_m" : "d0c48dd8-0887-4ac5-80e5-9b08c5dc77f1", 
                            "CreationDateTime" : ISODate("2022-04-18T23:29:00.840+0000"), 
                            "IsFrozen" : true, 
                            "LastValidManifest" : {
                                "_m" : "f7966611-14aa-4f18-92f4-8697b4927fb6", 
                                "CreationDateTime" : ISODate("2022-04-18T23:29:00.919+0000"), 
                                "_id" : ObjectId("625df43c74679c25b6c157ed"), 
                                "IsValid" : true, 
                                "Manifest" : {
                                    "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8", 
                                    "Hash" : "568863d1a27feb3682b720d43cebd723ee09ce57c538831bf94bafc9408871c9"
                                }, 
                                "Duration" : 420.0, 
                                "Thumbnail" : null, 
                                "Title" : "Mocked sample video"
                            }, 
                            "Owner" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("625df43c74679c25b6c157eb"), 
                                "SharedInfoId" : "625da02c2752994b203d3681"
                            }, 
                            "TotDownvotes" : NumberLong(1), 
                            "TotUpvotes" : NumberLong(2), 
                            "VideoManifests" : [
                                {
                                    "_m" : "1ca89e6c-716c-4936-b7dc-908c057a3e41", 
                                    "_id" : ObjectId("625df43c74679c25b6c157ed")
                                }
                            ]
                        }
                        """;

                    var expectedVideoMock = new Mock<Video>();
                    expectedVideoMock.Setup(v => v.Id).Returns("625df43c74679c25b6c157ec");
                    expectedVideoMock.Setup(v => v.CreationDateTime).Returns(new DateTime(2022, 04, 18, 23, 29, 00, 840));
                    expectedVideoMock.Setup(v => v.IsFrozen).Returns(true);
                    {
                        var manifest0Mock = new Mock<VideoManifest>();
                        manifest0Mock.Setup(m => m.Id).Returns("625df43c74679c25b6c157ed");
                        expectedVideoMock.Setup(v => v.LastValidManifest).Returns(manifest0Mock.Object);
                        expectedVideoMock.Setup(v => v.VideoManifests).Returns(new[] { manifest0Mock.Object });
                    }
                    {
                        var ownerMock = new Mock<User>();
                        ownerMock.Setup(u => u.Id).Returns("625df43c74679c25b6c157eb");
                        expectedVideoMock.Setup(v => v.Owner).Returns(ownerMock.Object);
                    }
                    expectedVideoMock.Setup(v => v.TotDownvotes).Returns(1);
                    expectedVideoMock.Setup(v => v.TotUpvotes).Returns(2);

                    tests.Add(new DeserializationTestElement<Video>(sourceDocument, expectedVideoMock.Object));
                }

                // "abfbd104-35ff-4429-9afc-79304a11efc0" - dev (pre v0.3.0), published for WAM event
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("6229f4e50a4567231a0ec7af"), 
                            "_m" : "abfbd104-35ff-4429-9afc-79304a11efc0", 
                            "CreationDateTime" : ISODate("2022-03-10T12:53:57.191+0000"), 
                            "EncryptionKey" : null, 
                            "EncryptionType" : "Plain", 
                            "Owner" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("6217ce348967891a527854e4"), 
                                "IdentityManifest" : {
                                    "Hash" : "581e7f32c667eedf975745652646a04cce04987a735a8af44c225f2ad085508e"
                                }, 
                                "SharedInfoId" : "61cdeb611235d8985b9d68ce"
                            }, 
                            "TotDownvotes" : NumberLong(1), 
                            "TotUpvotes" : NumberLong(2), 
                            "VideoManifests" : [
                                {
                                    "_m" : "f7966611-14aa-4f18-92f4-8697b4927fb6", 
                                    "CreationDateTime" : ISODate("2022-03-10T12:53:57.235+0000"), 
                                    "_id" : ObjectId("6229f475127a47231a0ec7b0"), 
                                    "IsValid" : true, 
                                    "Manifest" : {
                                        "Hash" : "8d14d87c6663d39fb5e57ae46963588c61eb0c9641dfac23cb3c37ec189d2634"
                                    }, 
                                    "Video" : {
                                        "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                        "_id" : ObjectId("6229f4e50a7a47231a7537af")
                                    }, 
                                    "Title" : "Test1"
                                }, 
                                {
                                    "_m" : "f7966611-14aa-4f18-92f4-8697b4927fb6", 
                                    "CreationDateTime" : ISODate("2022-03-10T13:17:57.667+0000"), 
                                    "_id" : ObjectId("6229fa8540452f3d336a34ee"), 
                                    "IsValid" : true, 
                                    "Manifest" : {
                                        "Hash" : "3ef2e441eac00e3685615bf90a16385b7aee97084f4cab6301bdbf76a1ed9d74"
                                    }, 
                                    "Video" : {
                                        "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                        "_id" : ObjectId("6229f4e50a7a47231a7537af")
                                    }, 
                                    "Title" : "Test2"
                                }
                            ]
                        }
                        """;

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
                        expectedVideoMock.Setup(v => v.LastValidManifest).Returns(manifest1Mock.Object);
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

                // "c32a815b-4667-4534-8276-eb3c1d812d09" - v0.3.9
                // invalid manifest
                {
                    var sourceDocument = 
                        $$"""
                        {
                            "_id" : ObjectId("64b407f64a709a0ceb86b07c"),
                            "_m" : "c32a815b-4667-4534-8276-eb3c1d812d09",
                            "CreationDateTime" : ISODate("2023-07-16T15:08:38.008+0000"),
                            "IsValid" : false,
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8",
                                "Hash" : "765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"
                            },
                            "Metadata" : null,
                            "ValidationErrors" : [
                                {
                                    "_m" : "f555eaa8-d8e1-4f23-a402-8b9ac5930832",
                                    "ErrorMessage" : "MissingTitle",
                                    "ErrorType" : "MissingTitle"
                                }
                            ],
                            "ValidationTime" : ISODate("2023-07-16T15:09:21.700+0000")
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("64b407f64a709a0ceb86b07c");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2023, 07, 16, 15, 08, 38, 008));
                    expectedManifestMock.Setup(m => m.IsValid).Returns(false);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"));
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(new[] { new ValidationError(ValidationErrorType.MissingTitle) });
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2023, 07, 16, 15, 09, 21, 700));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "c32a815b-4667-4534-8276-eb3c1d812d09" - v0.3.9
                // valid manifest v1
                {
                    var sourceDocument =
                        $$"""
                        {
                            "_id" : ObjectId("64b3ff1f70673319b83d4343"),
                            "_m" : "c32a815b-4667-4534-8276-eb3c1d812d09",
                            "CreationDateTime" : ISODate("2023-07-16T14:30:55.121+0000"),
                            "IsValid" : true,
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8",
                                "Hash" : "765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"
                            },
                            "Metadata" : {
                                "_m" : "8bc43b2f-985b-443c-9a16-e9420a8a1d9d",
                                "_t" : "VideoManifestMetadataV1",
                                "Description" : "Test description",
                                "Duration" : NumberLong(420),
                                "Sources" : [
                                    {
                                        "_m" : "ca9caff9-df18-4101-a362-f8f449bb2aac",
                                        "Bitrate" : NumberInt(560000),
                                        "Quality" : "720p",
                                        "Reference" : "5370D45B2CA38F480B53334163FEF3BEECD4D048B398852B33DD4F568C329956",
                                        "Size" : NumberLong(100000000)
                                    }
                                ],
                                "Thumbnail" : {
                                    "_m" : "91ce6fdc-b59a-46bc-9ad0-7a8608cdfa1c",
                                    "AspectRatio" : 1.7699999809265137,
                                    "Blurhash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj",
                                    "Sources" : {
                                        "480" : "A01F2EFCB975C9602F420700B7B39EC174B8378462B95871A1CC0EB786AAD2B6"
                                    }
                                },
                                "Title" : "Mocked sample video",
                                "BatchId" : "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                                "CreatedAt" : NumberLong(123456),
                                "UpdatedAt" : NumberLong(234567),
                                "PersonalData" : "{\"test\":\"sample\"}"
                            },
                            "ValidationErrors" : [

                            ],
                            "ValidationTime" : ISODate("2023-07-16T14:32:14.321+0000")
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("64b3ff1f70673319b83d4343");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2023, 07, 16, 14, 30, 55, 121));
                    expectedManifestMock.Setup(m => m.IsValid).Returns(true);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"));
                    {
                        var metadataMock = new Mock<VideoManifestMetadataV1>();
                        metadataMock.Setup(m => m.BatchId).Returns("36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f");
                        metadataMock.Setup(m => m.CreatedAt).Returns(123456);
                        metadataMock.Setup(m => m.Description).Returns("Test description");
                        metadataMock.Setup(m => m.Duration).Returns(420);
                        metadataMock.Setup(m => m.PersonalData).Returns("{\"test\":\"sample\"}");
                        metadataMock.Setup(m => m.Sources).Returns(new[]{
                            new VideoSourceV1(560000, "720p", "5370D45B2CA38F480B53334163FEF3BEECD4D048B398852B33DD4F568C329956", 100000000)
                        });
                        metadataMock.Setup(m => m.Thumbnail).Returns(new ThumbnailV1(1.7699999809265137f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string>
                        {
                            { "480", "A01F2EFCB975C9602F420700B7B39EC174B8378462B95871A1CC0EB786AAD2B6" },
                        }));
                        metadataMock.Setup(m => m.Title).Returns("Mocked sample video");
                        metadataMock.Setup(m => m.UpdatedAt).Returns(234567);

                        expectedManifestMock.Setup(m => m.Metadata).Returns(metadataMock.Object);
                    }
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(Array.Empty<ValidationError>());
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2023, 07, 16, 14, 32, 14, 321));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "c32a815b-4667-4534-8276-eb3c1d812d09" - v0.3.9
                // valid manifest v2
                {
                    var sourceDocument =
                        $$"""
                        {
                            "_id" : ObjectId("64b3ff1f70673319b83d4343"),
                            "_m" : "c32a815b-4667-4534-8276-eb3c1d812d09",
                            "CreationDateTime" : ISODate("2023-07-16T15:14:27.084+0000"),
                            "IsValid" : true,
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8",
                                "Hash" : "7fb7ed4960e636e91c9056dc7dd22d401ed8d4495b04df0a07379555784188a0"
                            },
                            "Metadata" : {
                                "_m" : "eff75fd8-54ea-437f-862b-782a153416bc",
                                "_t" : "VideoManifestMetadataV2",
                                "AspectRatio" : 1.7699999809265137,
                                "BatchId" : "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                                "CreatedAt" : NumberLong(123456),
                                "Description" : "Test description",
                                "Duration" : NumberLong(420),
                                "PersonalData" : "{\"test\":\"sample\"}",
                                "Sources" : [
                                    {
                                        "_m" : "91231db0-aded-453e-8178-f28a0a19776a",
                                        "Path" : "879448C8FD1B9895AB11708C6239F76352791483CA74A96F97D9F8FB177E9B87",
                                        "Quality" : "720",
                                        "Size" : NumberLong(100000000),
                                        "Type" : "mp4"
                                    }
                                ],
                                "Thumbnail" : {
                                    "_m" : "36966654-d85c-455b-b870-7b49e1124e6d",
                                    "AspectRatio" : 1.7699999809265137,
                                    "Blurhash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj",
                                    "Sources" : [
                                        {
                                            "_m" : "1fbae0a8-9ee0-40f0-a8ad-21a0083fcb66",
                                            "Path" : "479F24250AF8943DD1F52B3B3F514493E55A1E57FEC781B2E7720EB802A0B672",
                                            "Type" : "jpeg",
                                            "Width" : NumberInt(480)
                                        }
                                    ]
                                },
                                "Title" : "Mocked sample video",
                                "UpdatedAt" : NumberLong(234567)
                            },
                            "ValidationErrors" : [

                            ],
                            "ValidationTime" : ISODate("2023-07-16T15:14:27.404+0000")
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("64b3ff1f70673319b83d4343");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2023, 07, 16, 15, 14, 27, 084));
                    expectedManifestMock.Setup(m => m.IsValid).Returns(true);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("7fb7ed4960e636e91c9056dc7dd22d401ed8d4495b04df0a07379555784188a0"));
                    {
                        var metadataMock = new Mock<VideoManifestMetadataV2>();
                        metadataMock.Setup(m => m.AspectRatio).Returns(1.7699999809265137f);
                        metadataMock.Setup(m => m.BatchId).Returns("36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f");
                        metadataMock.Setup(m => m.CreatedAt).Returns(123456);
                        metadataMock.Setup(m => m.Description).Returns("Test description");
                        metadataMock.Setup(m => m.Duration).Returns(420);
                        metadataMock.Setup(m => m.PersonalData).Returns("{\"test\":\"sample\"}");
                        metadataMock.Setup(m => m.Sources).Returns(new[]{
                            new VideoSourceV2("879448C8FD1B9895AB11708C6239F76352791483CA74A96F97D9F8FB177E9B87", "720", 100000000, "mp4")
                        });
                        metadataMock.Setup(m => m.Thumbnail).Returns(new ThumbnailV2(1.7699999809265137f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new[]
                        {
                            new ImageSourceV2(480, "479F24250AF8943DD1F52B3B3F514493E55A1E57FEC781B2E7720EB802A0B672", "jpeg")
                        }));
                        metadataMock.Setup(m => m.Title).Returns("Mocked sample video");
                        metadataMock.Setup(m => m.UpdatedAt).Returns(234567);

                        expectedManifestMock.Setup(m => m.Metadata).Returns(metadataMock.Object);
                    }
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(Array.Empty<ValidationError>());
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2023, 07, 16, 15, 14, 27, 404));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e" - v0.3.4
                //invalid manifest
                {
                    var sourceDocument =
                        $$"""
                        {
                            "_id" : ObjectId("630fad72a93c1417a162417e"),
                            "_m" : "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e",
                            "CreationDateTime" : ISODate("2022-08-31T18:50:26.067+0000"),
                            "IsValid" : false,
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8",
                                "Hash" : "765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"
                            },
                            "ValidationErrors" : [
                                {
                                    "ErrorMessage": "MissingTitle",
                                    "ErrorType": "MissingTitle"
                                }
                            ],
                            "ValidationTime" : ISODate("2022-08-31T18:50:26.473+0000"),
                            "BatchId" : "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                            "Description" : "Test description",
                            "Duration" : NumberLong(420),
                            "OriginalQuality" : "720p",
                            "Sources" : [
                                {
                                    "_m" : "ca9caff9-df18-4101-a362-f8f449bb2aac",
                                    "Bitrate" : NumberInt(560000),
                                    "Quality" : "720p",
                                    "Reference" : "5370D45B2CA38F480B53334163FEF3BEECD4D048B398852B33DD4F568C329956",
                                    "Size" : NumberLong(100000000)
                                }
                            ],
                            "Thumbnail" : {
                                "_m" : "91ce6fdc-b59a-46bc-9ad0-7a8608cdfa1c",
                                "AspectRatio" : 1.7699999809265137,
                                "Blurhash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj",
                                "Sources" : {
                                    "480w" : "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"
                                }
                            }
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("630fad72a93c1417a162417e");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 08, 31, 18, 50, 26, 067));
                    expectedManifestMock.Setup(m => m.IsValid).Returns(false);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"));
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(new[] { new ValidationError(ValidationErrorType.MissingTitle) });
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 08, 31, 18, 50, 26, 473));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e" - v0.3.4
                //valid manifest
                {
                    var sourceDocument =
                        $$"""
                        {
                            "_id" : ObjectId("630fad72a93c1417a162417e"),
                            "_m" : "a48b92d6-c02d-4b1e-b1b0-0526c4bcaa6e",
                            "CreationDateTime" : ISODate("2022-08-31T18:50:26.067+0000"),
                            "IsValid" : true,
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8",
                                "Hash" : "765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"
                            },
                            "ValidationErrors" : [
                        
                            ],
                            "ValidationTime" : ISODate("2022-08-31T18:50:26.473+0000"),
                            "BatchId" : "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                            "Description" : "Test description",
                            "Duration" : NumberLong(420),
                            "OriginalQuality" : "720p",
                            "Sources" : [
                                {
                                    "_m" : "ca9caff9-df18-4101-a362-f8f449bb2aac",
                                    "Bitrate" : NumberInt(560000),
                                    "Quality" : "720p",
                                    "Reference" : "5370D45B2CA38F480B53334163FEF3BEECD4D048B398852B33DD4F568C329956",
                                    "Size" : NumberLong(100000000)
                                }
                            ],
                            "Thumbnail" : {
                                "_m" : "91ce6fdc-b59a-46bc-9ad0-7a8608cdfa1c",
                                "AspectRatio" : 1.7699999809265137,
                                "Blurhash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj",
                                "Sources" : {
                                    "480w" : "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"
                                }
                            },
                            "Title" : "Mocked sample video"
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("630fad72a93c1417a162417e");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 08, 31, 18, 50, 26, 067));
                    expectedManifestMock.Setup(m => m.IsValid).Returns(true);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("765a93649a58db3a4a85d800aa8111b13c7082e081b5ea186885d95cdd232dcb"));
                    {
                        var metadataMock = new Mock<VideoManifestMetadataV1>();
                        metadataMock.Setup(m => m.BatchId).Returns("36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f");
                        metadataMock.Setup(m => m.Description).Returns("Test description");
                        metadataMock.Setup(m => m.Duration).Returns(420);
                        metadataMock.Setup(m => m.Sources).Returns(new[]{
                            new VideoSourceV1(560000, "720p", "5370D45B2CA38F480B53334163FEF3BEECD4D048B398852B33DD4F568C329956", 100000000)
                        });
                        metadataMock.Setup(m => m.Thumbnail).Returns(new ThumbnailV1(1.7699999809265137f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string>
                        {
                            { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008" },
                        }));
                        metadataMock.Setup(m => m.Title).Returns("Mocked sample video");

                        expectedManifestMock.Setup(m => m.Metadata).Returns(metadataMock.Object);
                    }
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(Array.Empty<ValidationError>());
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 08, 31, 18, 50, 26, 473));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "dc33442b-ae1e-428b-8b63-5dafbf192ba8" - v0.3.0
                //invalid manifest
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("625df43c74679c25b6c157ed"), 
                            "_m" : "dc33442b-ae1e-428b-8b63-5dafbf192ba8", 
                            "CreationDateTime" : ISODate("2022-04-18T23:29:00.919+0000"), 
                            "ErrorValidationResults" : [
                                {
                                    "ErrorMessage": "MissingTitle",
                                    "ErrorType": "MissingTitle"
                                }
                            ], 
                            "IsValid" : false, 
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8", 
                                "Hash" : "568863d1a27feb3682b720d43cebd723ee09ce57c538831bf94bafc9408871c9"
                            }, 
                            "ValidationTime" : ISODate("2022-04-18T23:29:06.299+0000"), 
                            "Description" : "Test description",
                            "Duration" : 420.024, 
                            "OriginalQuality" : "720p", 
                            "Sources" : [
                                {
                                    "_m" : "ca9caff9-df18-4101-a362-f8f449bb2aac", 
                                    "Bitrate" : NumberInt(560000), 
                                    "Quality" : "720p", 
                                    "Reference" : "5FDAC6FCBBBC3CA5DBEAACFA0CF8F5777DB36793931E177D870C45E0D70CE637", 
                                    "Size" : NumberLong(100000000)
                                }
                            ], 
                            "Thumbnail" : {
                                "AspectRatio" : 1.7777777910232544, 
                                "BlurHash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj", 
                                "Sources" : {
                                    "480w" : "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"
                                }
                            }
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("625df43c74679c25b6c157ed");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 04, 18, 23, 29, 00, 919));
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(new[] { new ValidationError(ValidationErrorType.MissingTitle) });
                    expectedManifestMock.Setup(m => m.IsValid).Returns(false);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("568863d1a27feb3682b720d43cebd723ee09ce57c538831bf94bafc9408871c9"));
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 04, 18, 23, 29, 06, 299));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "dc33442b-ae1e-428b-8b63-5dafbf192ba8" - v0.3.0
                //valid manifest
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("625df43c74679c25b6c157ed"), 
                            "_m" : "dc33442b-ae1e-428b-8b63-5dafbf192ba8", 
                            "CreationDateTime" : ISODate("2022-04-18T23:29:00.919+0000"), 
                            "ErrorValidationResults" : [
                        
                            ], 
                            "IsValid" : true, 
                            "Manifest" : {
                                "_m" : "27edd50c-dd67-44d8-84ea-1eedcfe481e8", 
                                "Hash" : "568863d1a27feb3682b720d43cebd723ee09ce57c538831bf94bafc9408871c9"
                            }, 
                            "ValidationTime" : ISODate("2022-04-18T23:29:06.299+0000"), 
                            "Description" : "Test description",
                            "Duration" : 420.024, 
                            "OriginalQuality" : "720p", 
                            "Sources" : [
                                {
                                    "_m" : "ca9caff9-df18-4101-a362-f8f449bb2aac", 
                                    "Bitrate" : NumberInt(560000), 
                                    "Quality" : "720p", 
                                    "Reference" : "5FDAC6FCBBBC3CA5DBEAACFA0CF8F5777DB36793931E177D870C45E0D70CE637", 
                                    "Size" : NumberLong(100000000)
                                }
                            ], 
                            "Title" : "Mocked sample video", 
                            "Thumbnail" : {
                                "AspectRatio" : 1.7777777910232544, 
                                "BlurHash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj", 
                                "Sources" : {
                                    "480w" : "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008"
                                }
                            }
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("625df43c74679c25b6c157ed");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 04, 18, 23, 29, 00, 919));
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(Array.Empty<ValidationError>());
                    expectedManifestMock.Setup(m => m.IsValid).Returns(true);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("568863d1a27feb3682b720d43cebd723ee09ce57c538831bf94bafc9408871c9"));
                    {
                        var metadataMock = new Mock<VideoManifestMetadataV1>();
                        metadataMock.Setup(m => m.Description).Returns("Test description");
                        metadataMock.Setup(m => m.Duration).Returns(420);
                        metadataMock.Setup(m => m.Sources).Returns(new[]{
                            new VideoSourceV1(560000, "720p", "5FDAC6FCBBBC3CA5DBEAACFA0CF8F5777DB36793931E177D870C45E0D70CE637", 100000000)
                        });
                        metadataMock.Setup(m => m.Title).Returns("Mocked sample video");
                        metadataMock.Setup(m => m.Thumbnail).Returns(new ThumbnailV1(1.7777777910232544f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string>
                        {
                            { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008" },
                        }));

                        expectedManifestMock.Setup(m => m.Metadata).Returns(metadataMock.Object);
                    }
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 04, 18, 23, 29, 06, 299));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "ec578080-ccd2-4d49-8a76-555b10a5dad5" - dev (pre v0.3.0), published for WAM event
                //invalid manifest
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("622e619a0a7a47231a0ec7b5"), 
                            "_m" : "ec578080-ccd2-4d49-8a76-555b10a5dad5", 
                            "CreationDateTime" : ISODate("2022-03-13T21:26:50.359+0000"), 
                            "ErrorValidationResults" : [
                                {
                                    "ErrorMessage": "MissingTitle",
                                    "ErrorType": "MissingTitle"
                                }
                            ], 
                            "IsValid" : false, 
                            "Manifest" : {
                                "Hash" : "ce601b421535419ae5c536d736075afb9eaac39e304c75357ef9312251704232"
                            }, 
                            "ValidationTime" : ISODate("2022-03-13T21:26:50.455+0000"), 
                            "Description" : "Test description", 
                            "Duration" : 900.0054321289062, 
                            "OriginalQuality" : "720p", 
                            "Sources" : [
                                {
                                    "Bitrate" : NumberInt(557647), 
                                    "Quality" : "720p", 
                                    "Reference" : "d88f68aa5b157ce6bda355d8bd54179df264a899c03bf5bdf0d4569f20a6933b", 
                                    "Size" : NumberInt(62735710)
                                }
                            ], 
                            "Thumbnail" : {
                                "AspectRatio" : 1.7777777910232544, 
                                "BlurHash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj", 
                                "Sources" : {
                                    "1920w" : "5d2a835a77269dc7bb1fb6be7b12407326cf6dcde4bd14f41b92be9d82414421", 
                                    "480w" : "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008", 
                                    "960w" : "60f8f4b17cdae08da8d03f7fa3476f47d7d29517351ffe7bd9f171b929680009", 
                                    "1440w" : "7eb77f7d0c2d17d9e05036f154b4d26091ba3e7d0ccfe8ebda49cda2bb94cd9b"
                                }
                            }, 
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                "_id" : ObjectId("6229f4e50a7a47231a0ec7af")
                            }
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("622e619a0a7a47231a0ec7b5");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 03, 13, 21, 26, 50, 359));
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(new[] { new ValidationError(ValidationErrorType.MissingTitle) });
                    expectedManifestMock.Setup(m => m.IsValid).Returns(false);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("ce601b421535419ae5c536d736075afb9eaac39e304c75357ef9312251704232"));
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 03, 13, 21, 26, 50, 455));

                    tests.Add(new DeserializationTestElement<VideoManifest>(sourceDocument, expectedManifestMock.Object));
                }

                // "ec578080-ccd2-4d49-8a76-555b10a5dad5" - dev (pre v0.3.0), published for WAM event
                //valid manifest
                {
                    var sourceDocument =
                        $$"""
                        { 
                            "_id" : ObjectId("622e619a0a7a47231a0ec7b5"), 
                            "_m" : "ec578080-ccd2-4d49-8a76-555b10a5dad5", 
                            "CreationDateTime" : ISODate("2022-03-13T21:26:50.359+0000"), 
                            "ErrorValidationResults" : [
                        
                            ], 
                            "IsValid" : true, 
                            "Manifest" : {
                                "Hash" : "ce601b421535419ae5c536d736075afb9eaac39e304c75357ef9312251704232"
                            }, 
                            "ValidationTime" : ISODate("2022-03-13T21:26:50.455+0000"), 
                            "Description" : "Test description", 
                            "Duration" : 900.0054321289062, 
                            "OriginalQuality" : "720p", 
                            "Sources" : [
                                {
                                    "Bitrate" : NumberInt(557647), 
                                    "Quality" : "720p", 
                                    "Reference" : "d88f68aa5b157ce6bda355d8bd54179df264a899c03bf5bdf0d4569f20a6933b", 
                                    "Size" : NumberInt(62735710)
                                }
                            ], 
                            "Title" : "Etherna WAM presentation", 
                            "Thumbnail" : {
                                "AspectRatio" : 1.7777777910232544, 
                                "BlurHash" : "LEHV6nWB2yk8pyo0adR*.7kCMdnj", 
                                "Sources" : {
                                    "1920w" : "5d2a835a77269dc7bb1fb6be7b12407326cf6dcde4bd14f41b92be9d82414421", 
                                    "480w" : "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008", 
                                    "960w" : "60f8f4b17cdae08da8d03f7fa3476f47d7d29517351ffe7bd9f171b929680009", 
                                    "1440w" : "7eb77f7d0c2d17d9e05036f154b4d26091ba3e7d0ccfe8ebda49cda2bb94cd9b"
                                }
                            }, 
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                "_id" : ObjectId("6229f4e50a7a47231a0ec7af")
                            }
                        }
                        """;

                    var expectedManifestMock = new Mock<VideoManifest>();
                    expectedManifestMock.Setup(m => m.Id).Returns("622e619a0a7a47231a0ec7b5");
                    expectedManifestMock.Setup(m => m.CreationDateTime).Returns(new DateTime(2022, 03, 13, 21, 26, 50, 359));
                    expectedManifestMock.Setup(m => m.ValidationErrors).Returns(Array.Empty<ValidationError>());
                    expectedManifestMock.Setup(m => m.IsValid).Returns(true);
                    expectedManifestMock.Setup(m => m.Manifest).Returns(new SwarmBzz("ce601b421535419ae5c536d736075afb9eaac39e304c75357ef9312251704232"));
                    {
                        var metadataMock = new Mock<VideoManifestMetadataV1>();
                        metadataMock.Setup(m => m.Description).Returns("Test description");
                        metadataMock.Setup(m => m.Duration).Returns(900);
                        metadataMock.Setup(m => m.Sources).Returns(new[]{
                            new VideoSourceV1(557647, "720p", "d88f68aa5b157ce6bda355d8bd54179df264a899c03bf5bdf0d4569f20a6933b", 62735710)
                        });
                        metadataMock.Setup(m => m.Title).Returns("Etherna WAM presentation");
                        metadataMock.Setup(m => m.Thumbnail).Returns(new ThumbnailV1(1.7777777910232544f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string>
                        {
                            { "1920w", "5d2a835a77269dc7bb1fb6be7b12407326cf6dcde4bd14f41b92be9d82414421" },
                            { "480w", "a015d8923a777bf8230291318274a5f9795b4bb9445ad41a2667d06df1ea3008" },
                            { "960w", "60f8f4b17cdae08da8d03f7fa3476f47d7d29517351ffe7bd9f171b929680009" },
                            { "1440w", "7eb77f7d0c2d17d9e05036f154b4d26091ba3e7d0ccfe8ebda49cda2bb94cd9b" }
                        }));

                        expectedManifestMock.Setup(m => m.Metadata).Returns(metadataMock.Object);
                    }
                    expectedManifestMock.Setup(m => m.ValidationTime).Returns(new DateTime(2022, 03, 13, 21, 26, 50, 455));

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
                        $$"""
                        { 
                            "_id" : ObjectId("621e90110a7a47231a0ec797"), 
                            "_m" : "624955bf-8c09-427f-93da-fc6ddb9668a6", 
                            "CreationDateTime" : ISODate("2022-03-01T21:28:49.590+0000"), 
                            "Owner" : {
                                "_m" : "caa0968f-4493-485b-b8d0-bc40942e8684", 
                                "_id" : ObjectId("621d38a179200252573f108e"), 
                                "IdentityManifest" : {
                                    "Hash" : "07ca616dfd2f137455c386f377ee4647a99d6550af3033f9b5a12a9ed5262cf0"
                                }, 
                                "SharedInfoId" : "62189f757a067d558b7c4ec3"
                            }, 
                            "Value" : "Up", 
                            "Video" : {
                                "_m" : "d4844740-472d-48b9-b066-67ba9a2acc9b", 
                                "_id" : ObjectId("621d54ab0a7a47231a0ec78f")
                            }
                        }
                        """;

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
            Assert.Equal(testElement.ExpectedModel.TextHistory, result.TextHistory);
            Assert.Equal(testElement.ExpectedModel.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Author);
            Assert.NotNull(result.LastText);
            Assert.NotNull(result.Video);
        }

        [Theory, MemberData(nameof(ManualVideoReviewDeserializationTests))]
        public void ManualVideoReviewDeserialization(DeserializationTestElement<ManualVideoReview> testElement)
        {
            if (testElement is null)
                throw new ArgumentNullException(nameof(testElement));

            // Setup.
            using var documentReader = new JsonReader(testElement.SourceDocument);
            var modelMapSerializer = new ModelMapSerializer<ManualVideoReview>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            testElement.SetupAction(mongoDatabaseMock, dbContext);

            // Action.
            using var dbExecutionContext = new DbExecutionContextHandler(dbContext); //run into a db execution context
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(testElement.ExpectedModel.Id, result.Id);
            Assert.Equal(testElement.ExpectedModel.Author, result.Author, EntityModelEqualityComparer.Instance);
            Assert.Equal(testElement.ExpectedModel.CreationDateTime, result.CreationDateTime);
            Assert.Equal(testElement.ExpectedModel.Description, result.Description);
            Assert.Equal(testElement.ExpectedModel.IsValidResult, result.IsValidResult);
            Assert.Equal(testElement.ExpectedModel.Video, result.Video, EntityModelEqualityComparer.Instance);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.Author);
            Assert.NotNull(result.Description);
            Assert.NotNull(result.Video);
        }

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
            Assert.NotNull(result.Id);
            Assert.NotNull(result.SharedInfoId);
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
            Assert.Equal(testElement.ExpectedModel.LastValidManifest, result.LastValidManifest, EntityModelEqualityComparer.Instance);
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
            Assert.Equal(testElement.ExpectedModel.IsValid, result.IsValid);
            Assert.Equal(testElement.ExpectedModel.Manifest, result.Manifest);
            switch (testElement.ExpectedModel.Metadata)
            {
                case VideoManifestMetadataV1 expectedMetadataV1:
                    var resultMetadataV1 = result.Metadata as VideoManifestMetadataV1;
                    Assert.NotNull(resultMetadataV1);
                    Assert.Equal(expectedMetadataV1.BatchId, resultMetadataV1.BatchId);
                    Assert.Equal(expectedMetadataV1.CreatedAt, resultMetadataV1.CreatedAt);
                    Assert.Equal(expectedMetadataV1.Description, resultMetadataV1.Description);
                    Assert.Equal(expectedMetadataV1.Duration, resultMetadataV1.Duration);
                    Assert.Equal(expectedMetadataV1.PersonalData, resultMetadataV1.PersonalData);
                    Assert.Equal(expectedMetadataV1.Sources, resultMetadataV1.Sources);
                    Assert.Equal(expectedMetadataV1.Thumbnail, resultMetadataV1.Thumbnail);
                    Assert.Equal(expectedMetadataV1.Title, resultMetadataV1.Title);
                    Assert.Equal(expectedMetadataV1.UpdatedAt, resultMetadataV1.UpdatedAt);
                    Assert.NotNull(resultMetadataV1.Sources);
                    break;

                case VideoManifestMetadataV2 expectedMetadataV2:
                    var resultMetadataV2 = result.Metadata as VideoManifestMetadataV2;
                    Assert.NotNull(resultMetadataV2);
                    Assert.Equal(expectedMetadataV2.AspectRatio, resultMetadataV2.AspectRatio);
                    Assert.Equal(expectedMetadataV2.BatchId, resultMetadataV2.BatchId);
                    Assert.Equal(expectedMetadataV2.CreatedAt, resultMetadataV2.CreatedAt);
                    Assert.Equal(expectedMetadataV2.Description, resultMetadataV2.Description);
                    Assert.Equal(expectedMetadataV2.Duration, resultMetadataV2.Duration);
                    Assert.Equal(expectedMetadataV2.PersonalData, resultMetadataV2.PersonalData);
                    Assert.Equal(expectedMetadataV2.Sources, resultMetadataV2.Sources);
                    Assert.Equal(expectedMetadataV2.Thumbnail, resultMetadataV2.Thumbnail);
                    Assert.Equal(expectedMetadataV2.Title, resultMetadataV2.Title);
                    Assert.Equal(expectedMetadataV2.UpdatedAt, resultMetadataV2.UpdatedAt);
                    Assert.NotNull(resultMetadataV2.Sources);
                    break;

                case null:
                    Assert.Null(result.Metadata);
                    break;

                default: throw new InvalidOperationException();
            }
            Assert.Equal(testElement.ExpectedModel.ValidationErrors, result.ValidationErrors);
            Assert.Equal(testElement.ExpectedModel.ValidationTime, result.ValidationTime);
            Assert.NotNull(result.Id);
            Assert.NotNull(result.IsValid);
            Assert.NotNull(result.Manifest);
            Assert.NotNull(result.ValidationErrors);
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
