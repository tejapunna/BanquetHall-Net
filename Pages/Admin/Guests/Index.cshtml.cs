using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Guests;

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
        var query = _guestRepository.Query();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim().ToLower();
            query = query.Where(g =>
                g.Name.ToLower().Contains(term) ||
                g.Mobile.Contains(term) ||
                (g.Email != null && g.Email.ToLower().Contains(term)));
        }

        Guests = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
    }
}
