using Etherna.Authentication.Extensions;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Services.Domain;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    public class CommentsControllerService : ICommentsControllerService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;
        private readonly IUserService userService;

        // Constructor.
        public CommentsControllerService(
            IIndexDbContext dbContext,
            IUserService userService)
        {
            this.dbContext = dbContext;
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
        }
    }
}
