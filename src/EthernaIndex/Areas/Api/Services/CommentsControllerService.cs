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

using Etherna.Authentication.Extensions;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Services.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public class CommentsControllerService : ICommentsControllerService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly ILogger<CommentsControllerService> logger;
        private readonly IUserService userService;

        // Constructor.
        public CommentsControllerService(
            IIndexDbContext dbContext,
            ILogger<CommentsControllerService> logger,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.userService = userService;
        }

        // Methods.
        public async Task DeleteOwnedCommentAsync(string id, ClaimsPrincipal currentUserClaims)
        {
            // Get data.
            var address = currentUserClaims.GetEtherAddress();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var comment = await dbContext.Comments.FindOneAsync(id);

            // Verify authorization.
            if (comment.Author.Id != currentUser.Id)
                throw new UnauthorizedAccessException("User is not owner of the comment");

            // Action.
            comment.SetAsDeletedByAuthor();

            await dbContext.SaveChangesAsync();

            logger.DeletedCommentVideoByOwner(id);
        }
    }
}
