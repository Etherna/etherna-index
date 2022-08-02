using Etherna.EthernaIndex.Domain;
using Etherna.MongoDB.Driver;
using Moq;
using System;

namespace Etherna.EthernaIndex.Persistence.Helpers
{
    public class DeserializationTestElement<TModel>
    {
        public DeserializationTestElement(string sourceDocument, TModel expectedModel) :
            this(sourceDocument, expectedModel, (_, _) => { })
        { }

        public DeserializationTestElement(
            string sourceDocument,
            TModel expectedModel,
            Action<Mock<IMongoDatabase>, IIndexDbContext> setupAction)
        {
            SourceDocument = sourceDocument;
            ExpectedModel = expectedModel;
            SetupAction = setupAction;
        }

        public string SourceDocument { get; }
        public TModel ExpectedModel { get; }
        public Action<Mock<IMongoDatabase>, IIndexDbContext> SetupAction { get; }
    }
}
