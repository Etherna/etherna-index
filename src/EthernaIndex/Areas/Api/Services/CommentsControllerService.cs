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

using Etherna.Authentication;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Services.Domain;
using Etherna.EthernaIndex.Services.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public class CommentsControllerService : ICommentsControllerService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IEthernaOpenIdConnectClient ethernaOidcClient;
        private readonly ILogger<CommentsControllerService> logger;
        private readonly IUserService userService;

        // Constructor.
        public CommentsControllerService(
            IIndexDbContext dbContext,
            IEthernaOpenIdConnectClient ethernaOidcClient,
            ILogger<CommentsControllerService> logger,
            IUserService userService)
        {
            this.dbContext = dbContext;
            this.ethernaOidcClient = ethernaOidcClient;
            this.logger = logger;
            this.userService = userService;
        }

        // Methods.
        public async Task DeleteOwnedCommentAsync(string id)
        {
            // Get data.
            var address = await ethernaOidcClient.GetEtherAddressAsync();
            var (currentUser, _) = await userService.FindUserAsync(address);

            var comment = await dbContext.Comments.FindOneAsync(id);

            // Verify authorization.
            if (comment.Author.Id != currentUser.Id)
                throw new UnauthorizedAccessException("User is not owner of the comment");

            // Action.
            comment.SetAsDeletedByAuthor();

            await dbContext.SaveChangesAsync();

            logger.OwnerDeleteVideoComment(id);
        }
    }
}
