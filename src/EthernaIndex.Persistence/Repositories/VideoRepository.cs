using Digicando.MongODM.Repositories;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;

namespace Etherna.EthernaIndex.Persistence.Repositories
{
    class VideoRepository : CollectionRepositoryBase<Video, string>
    {
        public VideoRepository(IIndexContext dbContext)
            : base("videos", dbContext)
        { }
    }
}
