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
        private readonly IUserService userService;
        private readonly IVideoService videoService;

        // Constructor.
        public ModerationControllerService(
            IIndexDbContext dbContext,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            ILogger<ModerationControllerService> logger,
            IUserService userService,
            IVideoService videoService)
        {
            this.dbContext = dbContext;
            this.ethernaOidcClient = ethernaOidcClient;
            this.logger = logger;
            this.userService = userService;
            this.videoService = videoService;
        }

        // Methods.
        public async Task ModerateCommentAsync(string id)
        {
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (user, _) = await userService.FindUserAsync(address);

            var comment = await dbContext.Comments.FindOneAsync(id);
            comment.SetAsDeletedByModerator(user);
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
