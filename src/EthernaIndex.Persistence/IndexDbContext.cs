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
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Persistence.Repositories;
using Etherna.MongoDB.Driver;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Migration;
using Etherna.MongODM.Core.Repositories;
using Etherna.MongODM.Core.Serialization;
using Microsoft.Extensions.Logging;
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
        private const string ModelMapsNamespace = "Etherna.EthernaIndex.Persistence.ModelMaps.Index";

        // Constructor.
        public IndexDbContext(
            IEventDispatcher eventDispatcher,
            ILogger<IndexDbContext> logger)
            : base(logger)
        {
            EventDispatcher = eventDispatcher;
        }

        // Properties.
        //repositories
        public IRepository<Comment, string> Comments { get; } = new DomainRepository<Comment, string>(
            new RepositoryOptions<Comment>("comments")
            {
                IndexBuilders = new[]
                {
                    (Builders<Comment>.IndexKeys.Ascending(c => c.Video.Id), new CreateIndexOptions<Comment>())
                }
            });
        public IRepository<ManualVideoReview, string> ManualVideoReviews { get; } =
            new DomainRepository<ManualVideoReview, string>("manualVideoReviews");
        public IRepository<UnsuitableVideoReport, string> UnsuitableVideoReports { get; } =
            new DomainRepository<UnsuitableVideoReport, string>("unsuitableVideoReports");
        public IRepository<User, string> Users { get; } = new DomainRepository<User, string>(
            new RepositoryOptions<User>("users")
            {
                IndexBuilders = new[]
                {
                    (Builders<User>.IndexKeys.Ascending(u => u.SharedInfoId), new CreateIndexOptions<User> { Unique = true })
                }
            });
        public IRepository<VideoManifest, string> VideoManifests { get; } = new DomainRepository<VideoManifest, string>(
            new RepositoryOptions<VideoManifest>("videoManifests")
            {
                IndexBuilders = new[]
                {
                    (Builders<VideoManifest>.IndexKeys.Ascending(c => c.Manifest.Hash), new CreateIndexOptions<VideoManifest> { Unique = true }),
                    (Builders<VideoManifest>.IndexKeys.Descending(c => c.CreationDateTime), new CreateIndexOptions<VideoManifest>()),
                    (Builders<VideoManifest>.IndexKeys.Ascending(c => c.IsValid), new CreateIndexOptions<VideoManifest>())
                }
            });
        public IRepository<Video, string> Videos { get; } = new DomainRepository<Video, string>(
            new RepositoryOptions<Video>("videos")
            {
                IndexBuilders = new[]
                {
                    (Builders<Video>.IndexKeys.Descending(c => c.Owner.Id), new CreateIndexOptions<Video>()),
                }
            });
        public IRepository<VideoVote, string> Votes { get; } = new DomainRepository<VideoVote, string>(
            new RepositoryOptions<VideoVote>("votes")
            {
                IndexBuilders = new[]
                {
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Owner.Id)
                                                  .Ascending(v => v.Video.Id), new CreateIndexOptions<VideoVote>{ Unique = true }),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Video.Id), new CreateIndexOptions<VideoVote>()),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Value), new CreateIndexOptions<VideoVote>()),
                }
            });

        //other properties
        public override IEnumerable<DocumentMigration> DocumentMigrationList => new DocumentMigration[] {
            //v0.3.9
            new DocumentMigration<Video, string>(Videos),
            new DocumentMigration<VideoManifest, string>(VideoManifests)
        };
        public IEventDispatcher EventDispatcher { get; }

        // Protected properties.
        protected override IEnumerable<IModelMapsCollector> ModelMapsCollectors =>
            from t in typeof(IndexDbContext).GetTypeInfo().Assembly.GetTypes()
            where t.IsClass && t.Namespace == ModelMapsNamespace
            where t.GetInterfaces().Contains(typeof(IModelMapsCollector))
            select Activator.CreateInstance(t) as IModelMapsCollector;

        // Methods.
        public override async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changedEntityModels = ChangedModelsList.OfType<EntityModelBase>().ToArray();

            // Save changes.
            await base.SaveChangesAsync(cancellationToken);

            // Dispatch events.
            foreach (var model in changedEntityModels)
            {
                await EventDispatcher.DispatchAsync(model.Events);
                model.ClearEvents();
            }
        }
    }
}
