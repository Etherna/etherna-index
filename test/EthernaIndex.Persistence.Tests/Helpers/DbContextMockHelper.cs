using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core.Domain.Models;
using Etherna.MongODM.Core.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Etherna.EthernaIndex.Persistence.Helpers
{
    public static class DbContextMockHelper
    {
        public static Mock<IMongoCollection<TModel>> SetupCollectionMock<TModel, TKey>(
            Mock<IMongoDatabase> mongoDatabaseMock,
            ICollectionRepository<TModel, TKey> collection)
             where TModel : class, IEntityModel<TKey>
        {
            if (mongoDatabaseMock is null)
                throw new ArgumentNullException(nameof(mongoDatabaseMock));
            if (collection is null)
                throw new ArgumentNullException(nameof(collection));

            var collectionMock = new Mock<IMongoCollection<TModel>>();

            mongoDatabaseMock.Setup(d => d.GetCollection<TModel>(collection.Name, It.IsAny<MongoCollectionSettings>()))
                .Returns(() => collectionMock.Object);

            return collectionMock;
        }

        public static void SetupFindWithPredicate<TModel>(
            Mock<IMongoCollection<TModel>> collectionMock,
            Func<FilterDefinition<TModel>, IEnumerable<TModel>> modelSelector)
        {
            if (collectionMock is null)
                throw new ArgumentNullException(nameof(collectionMock));

            // Setup collection.
            collectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<TModel>>(),
                It.IsAny<FindOptions<TModel, TModel>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync<
                    FilterDefinition<TModel>,
                    FindOptions<TModel, TModel>,
                    CancellationToken,
                    IMongoCollection<TModel>,
                    IAsyncCursor<TModel>>((filter, _, _) =>
                {
                    // Setup cursor.
                    bool isFirstBatch = true;
                    var cursorMock = new Mock<IAsyncCursor<TModel>>();

                    cursorMock.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                        .ReturnsAsync(() =>
                        {
                            var wasFirstbatch = isFirstBatch;
                            isFirstBatch = false;
                            return wasFirstbatch;
                        });
                    cursorMock.Setup(c => c.Current)
                        .Returns(modelSelector(filter));

                    return cursorMock.Object;
                });
        }
    }
}
