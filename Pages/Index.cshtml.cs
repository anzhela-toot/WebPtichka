using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebPtichka.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login");
            }

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value?.ToLower();

            return role switch
            {
                "manager" => RedirectToPage("/Manager/Index"),
                "accountant" => RedirectToPage("/Accountant/Index"),
                _ => Page()
            };
        }
    }
}