using Digicando.DomainEvents;
using Digicando.MongODM;
using Digicando.MongODM.Repositories;
using Etherna.EthernaIndex.Domain.Models;

namespace Etherna.EthernaIndex.Domain
{
    public interface IIndexContext : IDbContext
    {
        ICollectionRepository<Channel, string> Channels { get; }
        ICollectionRepository<Video, string> Videos { get; }

        IEventDispatcher EventDispatcher { get; }
    }
}
