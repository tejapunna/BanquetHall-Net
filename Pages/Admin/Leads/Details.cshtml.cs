using BanquetHall.Data;
using BanquetHall.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Leads;

public class DetailsModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;

    public DetailsModel(BanquetHallDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Guest Guest { get; set; } = null!;
    public GuestFunction? LatestFunction { get; set; }
    public FollowUp? LatestFollowUp { get; set; }
    public string? ManagerName { get; set; }
    public string? FunctionNameText { get; set; }
    public string? FunctionHallText { get; set; }
    public string? AssignedManagerName { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await _dbContext.Guests
            .Include(g => g.Functions)
            .Include(g => g.FollowUps)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (guest == null)
            return NotFound();

        Guest = guest;

        LatestFunction = guest.Functions.OrderByDescending(f => f.FunctionDate).FirstOrDefault();
        LatestFollowUp = guest.FollowUps.OrderByDescending(f => f.UpdatedAt).FirstOrDefault();

        if (guest.InitiatedByManagerId.HasValue)
        {
            var mgr = await _dbContext.Users.FindAsync(guest.InitiatedByManagerId.Value);
            ManagerName = mgr?.FullName;
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

        return Page();
    }
}
