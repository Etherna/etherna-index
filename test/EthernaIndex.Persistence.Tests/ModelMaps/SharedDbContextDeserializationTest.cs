using Etherna.EthernaIndex.Domain.Models.UserAgg;
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
    public class SharedDbContextDeserializationTest
    {
        // Fields.
        private readonly SharedDbContext dbContext;

        // Constructor.
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Need to keep objects after test construction")]
        public SharedDbContextDeserializationTest()
        {
            // Setup dbContext.
            dbContext = new SharedDbContext();

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
        public static IEnumerable<object []> UserSharedInfoDeserializationTests
        {
            get
            {
                var tests = new List<(string sourceDocument, UserSharedInfo expectedUserSharedInfo)>();

                // "6d0d2ee1-6aa3-42ea-9833-ac592bfc6613" - from sso v0.3.0
                {
                    var sourceDocument =
                        @"{ 
                            ""_id"" : ObjectId(""61cdffb4fa7c4052d258adcb""), 
                            ""_m"" : ""6d0d2ee1-6aa3-42ea-9833-ac592bfc6613"", 
                            ""CreationDateTime"" : ISODate(""2021-12-30T18:51:32.706+0000""), 
                            ""EtherAddress"" : ""0x410211F4824A8f7EDf174B32AB215924557b4437"", 
                            ""EtherPreviousAddresses"" : [
                                ""0x6401ceD81d2e864f214A93823647F5baBF819123""
                            ], 
                            ""LockoutEnabled"" : true, 
                            ""LockoutEnd"" : ISODate(""2022-12-30T18:51:32.706+0000""),
                        }";

                    var expectedSharedInfoMock = new Mock<UserSharedInfo>();
                    expectedSharedInfoMock.Setup(i => i.Id).Returns("61cdffb4fa7c4052d258adcb");
                    expectedSharedInfoMock.Setup(i => i.CreationDateTime).Returns(new DateTime(2021, 12, 30, 18, 51, 32, 706));
                    expectedSharedInfoMock.Setup(i => i.EtherAddress).Returns("0x410211F4824A8f7EDf174B32AB215924557b4437");
                    expectedSharedInfoMock.Setup(i => i.EtherPreviousAddresses).Returns(new[] { "0x6401ceD81d2e864f214A93823647F5baBF819123" });
                    expectedSharedInfoMock.Setup(i => i.LockoutEnabled).Returns(true);
                    expectedSharedInfoMock.Setup(i => i.LockoutEnd).Returns(new DateTimeOffset(2022, 12, 30, 18, 51, 32, 706, TimeSpan.Zero));

                    tests.Add((sourceDocument, expectedSharedInfoMock.Object));
                }

                return tests.Select(t => new object[] { t.sourceDocument, t.expectedUserSharedInfo });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(UserSharedInfoDeserializationTests))]
        public void UserSharedInfoDeserialization(string sourceDocument, UserSharedInfo expectedUserSharedInfo)
        {
            if (sourceDocument is null)
                throw new ArgumentNullException(nameof(sourceDocument));
            if (expectedUserSharedInfo is null)
                throw new ArgumentNullException(nameof(expectedUserSharedInfo));

            // Setup.
            using var documentReader = new JsonReader(sourceDocument);
            var modelMapSerializer = new ModelMapSerializer<UserSharedInfo>(dbContext);
            var deserializationContext = BsonDeserializationContext.CreateRoot(documentReader);
            using var execContext = AsyncLocalContext.Instance.InitAsyncLocalContext(); //start an execution context

            // Action.
            var result = modelMapSerializer.Deserialize(deserializationContext);

            // Assert.
            Assert.Equal(expectedUserSharedInfo.Id, result.Id);
            Assert.Equal(expectedUserSharedInfo.CreationDateTime, result.CreationDateTime);
            Assert.Equal(expectedUserSharedInfo.EtherAddress, result.EtherAddress);
            Assert.Equal(expectedUserSharedInfo.EtherPreviousAddresses, result.EtherPreviousAddresses);
            Assert.Equal(expectedUserSharedInfo.IsLockedOutNow, result.IsLockedOutNow);
            Assert.Equal(expectedUserSharedInfo.LockoutEnabled, result.LockoutEnabled);
            Assert.Equal(expectedUserSharedInfo.LockoutEnd, result.LockoutEnd);
            Assert.NotNull(expectedUserSharedInfo.Id);
            Assert.NotNull(expectedUserSharedInfo.EtherAddress);
            Assert.NotNull(expectedUserSharedInfo.EtherPreviousAddresses);
        }
    }
}
