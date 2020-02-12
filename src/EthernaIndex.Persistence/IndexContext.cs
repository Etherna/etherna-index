using Digicando.DomainEvents;
using Digicando.MongODM;
using Digicando.MongODM.ProxyModels;
using Digicando.MongODM.Repositories;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Serialization.Modifiers;
using Digicando.MongODM.Utility;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Persistence.Repositories;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence
{
    public class IndexContext : DbContext, IIndexContext
    {
        // Consts.
        private const string SerializersNamespace = "Digicando.Bcerty.Server.Persistence.ClassMaps";

        // Constructor.
        public IndexContext(
            IDBCache dbCache,
            IDBMaintainer dbMaintainer,
            IDocumentSchemaRegister documentSchemaRegister,
            IEventDispatcher eventDispatcher,
            IProxyGenerator proxyGenerator,
            IOptionsMonitor<DbContextOptions> options,
            ISerializerModifierAccessor serializerModifierAccessor)
            : base(dbCache,
                  dbMaintainer,
                  documentSchemaRegister,
                  options.Get(nameof(IndexContext)),
                  proxyGenerator,
                  serializerModifierAccessor)
        {
            Channels = new ChannelRepository(this);
            Videos = new VideoRepository(this);

            ModelCollectionRepositoryMap = new Dictionary<Type, ICollectionRepository>
            {
                [typeof(Channel)] = Channels,
                [typeof(Video)] = Videos,
            };
            ModelGridFSRepositoryMap = new Dictionary<Type, IGridFSRepository>();
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<Channel, string> Channels { get; }
        public ICollectionRepository<Video, string> Videos { get; }

        public override IReadOnlyDictionary<Type, ICollectionRepository> ModelCollectionRepositoryMap { get; }
        public override IReadOnlyDictionary<Type, IGridFSRepository> ModelGridFSRepositoryMap { get; }

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelSerializerCollector> SerializerCollectors =>
            from t in typeof(IndexContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == SerializersNamespace
            where t.GetInterfaces().Contains(typeof(IModelSerializerCollector))
            select Activator.CreateInstance(t) as IModelSerializerCollector;

        // Methods.
        public override Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch events.
            foreach (var model in ChangedModelsList.Select(m => m as EntityModelBase))
            {
                EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
