using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace BanquetHall.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            return role switch
            {
                "Admin" => RedirectToPage("/Admin/Index"),
                "Manager" => RedirectToPage("/Manager/Index"),
                "Receptionist" => RedirectToPage("/Receptionist/Index"),
                _ => Page()
            };
        }
        return RedirectToPage("/Login");
    }
}
