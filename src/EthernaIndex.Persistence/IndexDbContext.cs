// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

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
    public class IndexDbContext(
        IEventDispatcher eventDispatcher,
        ILogger<IndexDbContext> logger)
        : DbContext(logger), IEventDispatcherDbContext, IIndexDbContext
    {
        // Consts.
        private const string ModelMapsNamespace = "Etherna.EthernaIndex.Persistence.ModelMaps.Index";

        // Properties.
        //repositories
        public IRepository<Comment, string> Comments { get; } = new DomainRepository<Comment, string>(
            new RepositoryOptions<Comment>("comments")
            {
                IndexBuilders =
                [
                    (Builders<Comment>.IndexKeys.Ascending(c => c.Video.Id), new CreateIndexOptions<Comment>())
                ]
            });
        public IRepository<ManualVideoReview, string> ManualVideoReviews { get; } =
            new DomainRepository<ManualVideoReview, string>("manualVideoReviews");
        public IRepository<UnsuitableVideoReport, string> UnsuitableVideoReports { get; } =
            new DomainRepository<UnsuitableVideoReport, string>("unsuitableVideoReports");
        public IRepository<User, string> Users { get; } = new DomainRepository<User, string>(
            new RepositoryOptions<User>("users")
            {
                IndexBuilders =
                [
                    (Builders<User>.IndexKeys.Ascending(u => u.SharedInfoId), new CreateIndexOptions<User> { Unique = true })
                ]
            });
        public IRepository<VideoManifest, string> VideoManifests { get; } = new DomainRepository<VideoManifest, string>(
            new RepositoryOptions<VideoManifest>("videoManifests")
            {
                IndexBuilders =
                [
                    (Builders<VideoManifest>.IndexKeys.Ascending(c => c.ManifestReference), new CreateIndexOptions<VideoManifest> { Unique = true }),
                    (Builders<VideoManifest>.IndexKeys.Descending(c => c.CreationDateTime), new CreateIndexOptions<VideoManifest>()),
                    (Builders<VideoManifest>.IndexKeys.Ascending(c => c.IsValid), new CreateIndexOptions<VideoManifest>())
                ]
            });
        public IRepository<Video, string> Videos { get; } = new DomainRepository<Video, string>(
            new RepositoryOptions<Video>("videos")
            {
                IndexBuilders =
                [
                    (Builders<Video>.IndexKeys.Descending(c => c.Owner.Id), new CreateIndexOptions<Video>())
                ]
            });
        public IRepository<VideoVote, string> Votes { get; } = new DomainRepository<VideoVote, string>(
            new RepositoryOptions<VideoVote>("votes")
            {
                IndexBuilders =
                [
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Owner.Id)
                                                  .Ascending(v => v.Video.Id), new CreateIndexOptions<VideoVote>{ Unique = true }),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Video.Id), new CreateIndexOptions<VideoVote>()),
                    (Builders<VideoVote>.IndexKeys.Ascending(v => v.Value), new CreateIndexOptions<VideoVote>())
                ]
            });

        //other properties
        public override IEnumerable<DocumentMigration> DocumentMigrationList =>
            [
                new DocumentMigration<VideoManifest, string>(VideoManifests)
            ];
        public IEventDispatcher EventDispatcher { get; } = eventDispatcher;

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
