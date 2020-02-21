using Digicando.MongODM.Repositories;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Persistence.Repositories
{
    class VideoRepository : CollectionRepositoryBase<Video, string>
    {
        public VideoRepository(IIndexContext dbContext)
            : base("videos", dbContext)
        { }

        protected override IEnumerable<(IndexKeysDefinition<Video> keys, CreateIndexOptions<Video> options)> IndexBuilders => new[]
        {
            (Builders<Video>.IndexKeys.Ascending(c => c.VideoHash),
             new CreateIndexOptions<Video> { Unique = true })
        };
    }
}
