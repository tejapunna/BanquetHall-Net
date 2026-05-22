using BanquetHall.Data;
using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Guests;

public class DetailsModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;
    private readonly IPaymentRepository _paymentRepository;

    public DetailsModel(BanquetHallDbContext dbContext, IPaymentRepository paymentRepository)
    {
        _dbContext = dbContext;
        _paymentRepository = paymentRepository;
    }

    public Guest Guest { get; set; } = null!;
    public GuestFunction? LatestFunction { get; set; }
    public List<FollowUp> FollowUps { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
    public string? FunctionNameText { get; set; }
    public string? FunctionHallText { get; set; }
    public string? AssignedManagerName { get; set; }
    public string? InitiatedByManager { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await _dbContext.Guests
            .Include(g => g.Functions)
            .Include(g => g.FollowUps)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (guest == null) return NotFound();

        Guest = guest;
        LatestFunction = guest.Functions.OrderByDescending(f => f.FunctionDate).FirstOrDefault();

        if (guest.InitiatedByManagerId.HasValue)
        {
            var mgr = await _dbContext.Users.FindAsync(guest.InitiatedByManagerId.Value);
            InitiatedByManager = mgr?.FullName;
        }

        if (LatestFunction != null)
        {
            if (LatestFunction.FunctionNameId.HasValue)
            {
                var fn = await _dbContext.FunctionNames.FindAsync(LatestFunction.FunctionNameId.Value);
                FunctionNameText = fn?.Name;
            }
            if (LatestFunction.FunctionHallId.HasValue)
            {
                var fh = await _dbContext.FunctionHalls.FindAsync(LatestFunction.FunctionHallId.Value);
                FunctionHallText = fh?.Name;
            }
            if (LatestFunction.AssignedManagerId.HasValue)
            {
                var am = await _dbContext.Users.FindAsync(LatestFunction.AssignedManagerId.Value);
                AssignedManagerName = am?.FullName;
            }
        }

        Payments = (await _paymentRepository.GetByGuestIdAsync(id)).ToList();

        FollowUps = await _dbContext.FollowUps
            .Include(f => f.Manager)
            .Where(f => f.GuestId == id)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();

        return Page();
    }
}
