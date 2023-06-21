//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.Authentication;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Persistence;
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
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly ILogger<ModerationControllerService> logger;
        private readonly IVideoService videoService;
        private readonly IUserService userService;

        // Constructor.
        public ModerationControllerService(
            IIndexDbContext dbContext,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            ILogger<ModerationControllerService> logger,
            IVideoService videoService,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.ethernaOidcClient = ethernaOidcClient;
            this.logger = logger;
            this.videoService = videoService;
            this.userService = userService;
        }

        // Methods.
        public async Task ModerateCommentAsync(string id)
        {
            var comment = await dbContext.Comments.FindOneAsync(id);
            comment.SetAsDeletedByModerator();
            await dbContext.SaveChangesAsync();

            logger.ModerateComment(id);
        }

        public async Task ModerateVideoAsync(string id, string description)
        {
            // Get current user.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, sharedInfo) = await userService.FindUserAsync(address);

            // Getvideo to moderate.
            var video = await dbContext.Videos.FindOneAsync(id);

            // Create review.
            var manualVideoReview = new ManualVideoReview(user, description, false, video);
            await dbContext.ManualVideoReviews.CreateAsync(manualVideoReview);

            // Moderate and save.
            await videoService.ModerateUnsuitableVideoAsync(video, manualVideoReview);
            await dbContext.SaveChangesAsync();

            logger.ModerateVideo(id);
        }
    }
}
