using Etherna.DomainEvents;
using Etherna.MongODM;

namespace Etherna.EthernaIndex.Persistence
{
    public interface IEventDispatcherDbContext : IDbContext
    {
        IEventDispatcher EventDispatcher { get; }
    }
}
