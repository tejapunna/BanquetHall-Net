using BanquetHall.Services;
using BanquetHall.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Receptionist.Guests;

public class DetailsModel : PageModel
{
    private readonly IGuestService _guestService;

    public DetailsModel(IGuestService guestService)
    {
        _guestService = guestService;
    }

    public GuestDetailDto? GuestDetail { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        GuestDetail = await _guestService.GetGuestDetailAsync(id, "Receptionist");
        if (GuestDetail == null)
            return NotFound();

        return Page();
    }
}
