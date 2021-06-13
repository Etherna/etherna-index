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
    public class IndexContext : DbContext, IEventDispatcherDbContext, IIndexContext
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
        public ICollectionRepository<Comment, string> Comments { get; } = new DomainCollectionRepository<Comment, string>(
            new CollectionRepositoryOptions<Comment>("comments")
            {
                IndexBuilders = new[]
                {
                    (Builders<Comment>.IndexKeys.Ascending(c => c.Video.ManifestHash.Hash), new CreateIndexOptions<Comment>())
                }
            });
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
        public ICollectionRepository<VideoVote, string> Votes { get; } = new DomainCollectionRepository<VideoVote, string>(
            new CollectionRepositoryOptions<VideoVote>("votes")
            {
                IndexBuilders = new[]
                {
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Owner.Address)
                                                  .Ascending(v => v.Video.ManifestHash.Hash), new CreateIndexOptions<VideoVote>{ Unique = true }),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Video.ManifestHash.Hash), new CreateIndexOptions<VideoVote>()),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Value), new CreateIndexOptions<VideoVote>()),
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
