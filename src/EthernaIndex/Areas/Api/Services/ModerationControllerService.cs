using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Services.Domain;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public class ModerationControllerService : IModerationControllerService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IVideoService videoService;

        // Constructor.
        public ModerationControllerService(
            IIndexDbContext dbContext,
            IVideoService videoService)
        {
            this.dbContext = dbContext;
            this.videoService = videoService;
        }

        // Methods.
        public async Task ModerateCommentAsync(string id)
        {
            var comment = await dbContext.Comments.FindOneAsync(id);
            comment.SetAsUnsuitable();
            await dbContext.SaveChangesAsync();
        }

        public async Task ModerateVideoAsync(string id)
        {
            var video = await dbContext.Videos.FindOneAsync(id);
            await videoService.ModerateUnsuitableVideoAsync(video);
        }
    }
}
