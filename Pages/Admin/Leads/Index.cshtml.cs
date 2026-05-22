using BanquetHall.Data;
using BanquetHall.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Leads;

public class IndexModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;

    public IndexModel(BanquetHallDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<LeadViewModel> Leads { get; set; } = new();
    public List<User> Managers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? LeadDateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? LeadDateTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FunctionDateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FunctionDateTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ManagerId { get; set; }

    public async Task OnGetAsync()
    {
        Managers = await _dbContext.Users
            .Where(u => u.Role == "Manager" && u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var query = _dbContext.Guests
            .Include(g => g.Functions)
            .Include(g => g.FollowUps)
            .AsQueryable();

        if (LeadDateFrom.HasValue)
            query = query.Where(g => g.CreatedAt.Date >= LeadDateFrom.Value.Date);

        if (LeadDateTo.HasValue)
            query = query.Where(g => g.CreatedAt.Date <= LeadDateTo.Value.Date);

        if (FunctionDateFrom.HasValue)
            query = query.Where(g => g.Functions.Any(f => f.FunctionDate.Date >= FunctionDateFrom.Value.Date));

        if (FunctionDateTo.HasValue)
            query = query.Where(g => g.Functions.Any(f => f.FunctionDate.Date <= FunctionDateTo.Value.Date));

        if (ManagerId.HasValue)
            query = query.Where(g => g.InitiatedByManagerId == ManagerId.Value);

        var guests = await query.OrderByDescending(g => g.CreatedAt).ToListAsync();

        // Resolve manager names
        var managerIds = guests
            .Where(g => g.InitiatedByManagerId.HasValue)
            .Select(g => g.InitiatedByManagerId!.Value)
            .Distinct()
            .ToList();

        var managerMap = await _dbContext.Users
            .Where(u => managerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

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
                Status = g.Status ?? "New",
                ManagedBy = g.InitiatedByManagerId.HasValue && managerMap.ContainsKey(g.InitiatedByManagerId.Value)
                    ? managerMap[g.InitiatedByManagerId.Value]
                    : "-",
                LastFollowUpDate = latestFollowUp?.UpdatedAt
            };
        }).ToList();
    }

    public class LeadViewModel
    {
        public int GuestId { get; set; }
        public DateTime LeadDate { get; set; }
        public DateTime? FunctionDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ManagedBy { get; set; } = string.Empty;
        public DateTime? LastFollowUpDate { get; set; }
    }
}
