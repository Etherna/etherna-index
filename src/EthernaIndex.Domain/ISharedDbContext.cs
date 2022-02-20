using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.EthernaIndex.Domain
{
    /// <summary>
    /// Shared DbContext between Etherna services. It's managed by SSO Server, use in read-only mode.
    /// </summary>
    public interface ISharedDbContext : IDbContext
    {
        ICollectionRepository<UserSharedInfo, string> UsersInfo { get; }
    }
}
