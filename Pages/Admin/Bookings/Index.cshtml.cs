using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Bookings;

public class IndexModel : PageModel
{
    private readonly IGuestFunctionRepository _guestFunctionRepository;
    private readonly IUserRepository _userRepository;

    public IndexModel(IGuestFunctionRepository guestFunctionRepository, IUserRepository userRepository)
    {
        _guestFunctionRepository = guestFunctionRepository;
        _userRepository = userRepository;
    }

    [BindProperty(SupportsGet = true)]
    public string? ManagerFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public List<GuestFunction> Bookings { get; set; } = new();
    public List<User> Managers { get; set; } = new();

    public async Task OnGetAsync()
    {
        Managers = (await _userRepository.GetByRoleAsync("Manager")).ToList();

        IQueryable<GuestFunction> query = _guestFunctionRepository.Query()
            .Include(gf => gf.Guest);

        if (!string.IsNullOrWhiteSpace(ManagerFilter))
        {
            query = query.Where(gf => gf.InitiatedBy == ManagerFilter);
        }

        if (FromDate.HasValue)
        {
            query = query.Where(gf => gf.FunctionDate >= FromDate.Value);
        }

        if (ToDate.HasValue)
        {
            query = query.Where(gf => gf.FunctionDate <= ToDate.Value);
        }

        Bookings = await query.OrderByDescending(gf => gf.CreatedAt).ToListAsync();
    }
}
