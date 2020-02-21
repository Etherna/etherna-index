using Digicando.MongODM.Repositories;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Persistence.Repositories
{
    class ChannelRepository : CollectionRepositoryBase<Channel, string>
    {
        public ChannelRepository(IIndexContext dbContext)
            : base("channels", dbContext)
        { }

        protected override IEnumerable<(IndexKeysDefinition<Channel> keys, CreateIndexOptions<Channel> options)> IndexBuilders => new[]
        {
            (Builders<Channel>.IndexKeys.Ascending(c => c.Address),
             new CreateIndexOptions<Channel> { Unique = true })
        };
    }
}
