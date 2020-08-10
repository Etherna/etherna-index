using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Account.Pages
{
    [Authorize]
    public class LoginModel : PageModel
    {
        // Fields.
        private readonly IIndexContext indexContext;

        // Constructors.
        public LoginModel(IIndexContext indexContext)
        {
            this.indexContext = indexContext;
        }

        // Methods.
        public async Task<IActionResult> OnGetAsync()
        {
            var address = User.GetEtherAddress();
            var prevAddresses = User.GetEtherPrevAddresses();

            // Verify if user exists.
            var user = await indexContext.Users.QueryElementsAsync(elements =>
                elements.Where(u => u.Address == address ||
                                    prevAddresses.Contains(u.Address))
                        .FirstOrDefaultAsync());

            // Create if it doesn't exist.
            if (user is null)
            {
                user = new User(address);
                await indexContext.Users.CreateAsync(user);
            }

            // Check if user have changed address.
            if (address != user.Address)
            {
                //migrate
                throw new NotImplementedException();
            }

            return RedirectToPage("/Index");
        }
    }
}
