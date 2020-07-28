using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.EthernaIndex.Areas.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet() =>
            SignOut("Cookies", "oidc");
    }
}
