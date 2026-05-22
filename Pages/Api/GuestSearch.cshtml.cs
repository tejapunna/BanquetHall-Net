using System.Security.Claims;
using BanquetHall.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Api;

[Authorize]
public class GuestSearchModel : PageModel
{
    private readonly IGuestService _guestService;

    public GuestSearchModel(IGuestService guestService)
    {
        _guestService = guestService;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnGetSearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            return new JsonResult(new List<object>());

        var results = await _guestService.SearchGuestsAsync(term);
        return new JsonResult(results);
    }
}
