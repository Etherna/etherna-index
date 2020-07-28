using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Etherna.EthernaIndex.Areas.Account.Pages
{
    [Authorize]
    public class LoginModel : PageModel
    {
        public IActionResult OnGet() =>
            RedirectToPage("/Index");
    }
}
