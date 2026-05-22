using System.Security.Claims;
using BanquetHall.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Api;

[Authorize]
public class AutofillModel : PageModel
{
    private readonly IGuestService _guestService;

    public AutofillModel(IGuestService guestService)
    {
        _guestService = guestService;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnGetGuestDetailAsync(int id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Receptionist";
        var detail = await _guestService.GetGuestDetailAsync(id, role);
        if (detail == null)
            return new JsonResult(new { error = "Guest not found" });

        return new JsonResult(detail);
    }
}
