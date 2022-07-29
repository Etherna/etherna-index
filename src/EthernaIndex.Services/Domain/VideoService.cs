using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    class VideoService : IVideoService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;

        // Constructor.
        public VideoService(IIndexDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Methods.
        public async Task DeleteVideoAsync(Video video)
        {
            // Delete manifests.
            foreach (var manifest in video.VideoManifests)
                await dbContext.VideoManifests.DeleteAsync(manifest);

            // Delete video.
            await dbContext.Videos.DeleteAsync(video);
        }

        public async Task ModerateUnsuitableVideoAsync(Video video)
        {
            // Delete unsitable manifests.
            //save manifest list
            var videoManifests = video.VideoManifests.ToList();

            //set video as unsuitable
            video.SetAsUnsuitable();
            await dbContext.SaveChangesAsync();

            //remove all VideoManifests from database
            foreach (var videoManifest in videoManifests)
                await dbContext.VideoManifests.DeleteAsync(videoManifest);
        }
    }
}
