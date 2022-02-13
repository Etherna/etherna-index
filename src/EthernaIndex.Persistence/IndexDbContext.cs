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
using Etherna.EthernaIndex.Persistence.Repositories;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence
{
    public class IndexDbContext : DbContext, IEventDispatcherDbContext, IIndexDbContext
    {
        // Consts.
        private const string SerializersNamespace = "Etherna.EthernaIndex.Persistence.ModelMaps";

        // Constructor.
        public IndexDbContext(
            IEventDispatcher eventDispatcher)
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
                    (Builders<Comment>.IndexKeys.Ascending(c => c.Video.Id), new CreateIndexOptions<Comment>())
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
                    (Builders<Video>.IndexKeys.Ascending(v => v.ContentReview), new CreateIndexOptions<Video>()),
                }
            });
        public ICollectionRepository<VideoManifest, string> VideoManifests { get; } = new DomainCollectionRepository<VideoManifest, string>(
            new CollectionRepositoryOptions<VideoManifest>("videoManifests")
            {
                IndexBuilders = new[]
                {
                    (Builders<VideoManifest>.IndexKeys.Ascending(c => c.ManifestHash.Hash), new CreateIndexOptions<VideoManifest> { Unique = true })
                }
            });
        public ICollectionRepository<VideoReport, string> VideoReports { get; } = new DomainCollectionRepository<VideoReport, string>("videoReports");
        public ICollectionRepository<VideoReview, string> VideoReviews { get; } = new DomainCollectionRepository<VideoReview, string>("videoReviews");
        public ICollectionRepository<VideoVote, string> Votes { get; } = new DomainCollectionRepository<VideoVote, string>(
            new CollectionRepositoryOptions<VideoVote>("votes")
            {
                IndexBuilders = new[]
                {
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Owner.Address)
                                                  .Ascending(v => v.Video.Id), new CreateIndexOptions<VideoVote>{ Unique = true }),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Video.Id), new CreateIndexOptions<VideoVote>()),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Value), new CreateIndexOptions<VideoVote>()),
                }
            });

        //other properties
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(IndexDbContext).GetTypeInfo().Assembly.GetTypes()
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
