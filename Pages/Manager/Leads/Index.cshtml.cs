using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Leads;

public class IndexModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;

    public IndexModel(BanquetHallDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<LeadViewModel> Leads { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? DateType { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public async Task OnGetAsync()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var assignedGuestIds = await _dbContext.GuestFunctions
            .Where(f => f.AssignedManagerId == managerId)
            .Select(f => f.GuestId)
            .Distinct()
            .ToListAsync();

        var query = _dbContext.Guests
            .Include(g => g.Functions)
            .Include(g => g.FollowUps)
            .Where(g => g.InitiatedByManagerId == managerId || assignedGuestIds.Contains(g.Id))
            .AsQueryable();

        // Apply date filter based on dropdown selection
        if (FromDate.HasValue || ToDate.HasValue)
        {
            if (DateType == "FunctionDate")
            {
                if (FromDate.HasValue)
                    query = query.Where(g => g.Functions.Any(f => f.FunctionDate.Date >= FromDate.Value.Date));
                if (ToDate.HasValue)
                    query = query.Where(g => g.Functions.Any(f => f.FunctionDate.Date <= ToDate.Value.Date));
            }
            else // LeadDate (default)
            {
                if (FromDate.HasValue)
                    query = query.Where(g => g.CreatedAt.Date >= FromDate.Value.Date);
                if (ToDate.HasValue)
                    query = query.Where(g => g.CreatedAt.Date <= ToDate.Value.Date);
            }
        }

        var guests = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

        Leads = guests.Select(g =>
        {
            var latestFunction = g.Functions.OrderByDescending(f => f.FunctionDate).FirstOrDefault();
            var latestFollowUp = g.FollowUps.OrderByDescending(f => f.UpdatedAt).FirstOrDefault();
            return new LeadViewModel
            {
                GuestId = g.Id,
                LeadDate = g.CreatedAt,
                FunctionDate = latestFunction?.FunctionDate,
                Name = g.Name,
                Phone = g.Mobile,
                Status = latestFollowUp?.FollowupStatus ?? "New",
                LastFollowUpDate = latestFollowUp?.UpdatedAt,
                NextFollowUpDate = latestFollowUp?.FollowupDate
            };
        }).ToList();
    }

    public class LeadViewModel
    {
        public int GuestId { get; set; }
        public DateTime LeadDate { get; set; }
        public DateTime? FunctionDate { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? LastFollowUpDate { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
    }
}
