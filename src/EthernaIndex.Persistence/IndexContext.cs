using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Persistence.Repositories;
using Etherna.MongODM;
using Etherna.MongODM.Repositories;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Utility;
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
        private const string SerializersNamespace = "Etherna.EthernaIndex.Persistence.ModelMaps";

        // Constructor.
        public IndexContext(
            IDbDependencies dbDependencies,
            IEventDispatcher eventDispatcher,
            DbContextOptions<IndexContext> options)
            : base(dbDependencies, options)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public ICollectionRepository<User, string> Users { get; } = new DomainCollectionRepository<User, string>(
            new CollectionRepositoryOptions<User>("users")
            {
                IndexBuilders = new[]
                {
                    (Builders<User>.IndexKeys.Ascending(u => u.Address), new CreateIndexOptions<User> { Unique = true }),
                    (Builders<User>.IndexKeys.Ascending(u => u.IdentityManifest!.Hash), new CreateIndexOptions<User>{ Sparse = true, Unique = true })
                }
            });
        public ICollectionRepository<Video, string> Videos { get; } = new DomainCollectionRepository<Video, string>(
            new CollectionRepositoryOptions<Video>("videos")
            {
                IndexBuilders = new[]
                {
                    (Builders<Video>.IndexKeys.Ascending(c => c.ManifestHash.Hash), new CreateIndexOptions<Video> { Unique = true })
                }
            });

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(IndexContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == SerializersNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Dispatch events.
            foreach (var model in ChangedModelsList.Where(m => m is EntityModelBase)
                                                   .Select(m => (EntityModelBase)m))
            {
                EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
