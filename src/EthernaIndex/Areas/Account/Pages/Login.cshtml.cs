using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver.Linq;
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

            // Create user if it doesn't exist.
            var user = await indexContext.Users.QueryElementsAsync(elements =>
                elements.FirstOrDefaultAsync(c => c.Address == address));

            if (user is null)
            {
                user = new User(address);
                await indexContext.Users.CreateAsync(user);
            }

            return RedirectToPage("/Index");
        }
    }
}
