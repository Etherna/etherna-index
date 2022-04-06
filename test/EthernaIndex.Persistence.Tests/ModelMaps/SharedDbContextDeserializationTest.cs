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
        }
    }
}
