using Digicando.MongODM.Repositories;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;

namespace Etherna.EthernaIndex.Persistence.Repositories
{
    class ChannelRepository : CollectionRepositoryBase<Channel, string>
    {
        public ChannelRepository(IIndexContext dbContext)
            : base("channels", dbContext)
        { }
    }
}
