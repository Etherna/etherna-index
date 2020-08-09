using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Repositories;

namespace Etherna.EthernaIndex.Domain
{
    public interface IIndexContext : IDbContext
    {
        ICollectionRepository<User, string> Users { get; }
        ICollectionRepository<Video, string> Videos { get; }

        IEventDispatcher EventDispatcher { get; }
    }
}
