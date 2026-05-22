using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Receptionist.Guests;

public class IndexModel : PageModel
{
    private readonly IGuestRepository _guestRepository;

    public IndexModel(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public List<Guest> Guests { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(SearchTerm) && SearchTerm.Length >= 3)
        {
            var results = await _guestRepository.SearchAsync(SearchTerm);
            Guests = results.ToList();
        }
    }
}
