// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Extensions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public class ModerationControllerService : IModerationControllerService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly ILogger<ModerationControllerService> logger;
        private readonly IVideoService videoService;

        // Constructor.
        public ModerationControllerService(
            IIndexDbContext dbContext,
            ILogger<ModerationControllerService> logger,
            IVideoService videoService)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.videoService = videoService;
        }

        // Methods.
        public async Task ModerateCommentAsync(string id)
        {
            var comment = await dbContext.Comments.FindOneAsync(id);
            comment.SetAsDeletedByModerator();
            await dbContext.SaveChangesAsync();

            logger.ModerateComment(id);
        }

        public async Task ModerateVideoAsync(string id)
        {
            var video = await dbContext.Videos.FindOneAsync(id);
            await videoService.ModerateUnsuitableVideoAsync(video);

            logger.ModerateVideo(id);
        }
    }
}
