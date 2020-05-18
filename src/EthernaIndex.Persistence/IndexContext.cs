using Digicando.DomainEvents;
using Digicando.MongODM;
using Digicando.MongODM.Repositories;
using Digicando.MongODM.Serialization;
using Digicando.MongODM.Utility;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using MongoDB.Driver;
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
        private const string SerializersNamespace = "Etherna.EthernaIndex.Persistence.ClassMaps";

        // Constructor.
        public IndexContext(
            IDbContextDependencies dbContextDependencies,
            IEventDispatcher eventDispatcher,
            DbContextOptions<IndexContext> options)
            : base(dbContextDependencies, options)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<Channel, string> Channels { get; } = new CollectionRepository<Channel, string>(
            new CollectionRepositoryOptions<Channel>("channels")
            {
                IndexBuilders = new[]
                {
                    (Builders<Channel>.IndexKeys.Ascending(c => c.Address), new CreateIndexOptions<Channel> { Unique = true })
                }
            });
        public ICollectionRepository<Video, string> Videos { get; } = new CollectionRepository<Video, string>(
            new CollectionRepositoryOptions<Video>("videos")
            {
                IndexBuilders = new[]
                {
                    (Builders<Video>.IndexKeys.Ascending(c => c.VideoHash), new CreateIndexOptions<Video> { Unique = true })
                }
            });

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
            foreach (var model in ChangedModelsList.Select(m => (EntityModelBase)m))
            {
                EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
