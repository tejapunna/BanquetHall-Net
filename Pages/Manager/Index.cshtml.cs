using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager;

public class IndexModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;
    private readonly IGuestRepository _guestRepository;

    public IndexModel(BanquetHallDbContext dbContext, IGuestRepository guestRepository)
    {
        _dbContext = dbContext;
        _guestRepository = guestRepository;
    }

    public string ManagerName { get; set; } = string.Empty;
    public int ActiveLeadsCount { get; set; }
    public List<ActiveLeadViewModel> ActiveLeads { get; set; } = new();
    public List<Guest> IncompleteBookings { get; set; } = new();

    public async Task OnGetAsync()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ManagerName = User.FindFirst("FullName")?.Value ?? "Manager";

        // Get unique active leads (one per guest, latest follow-up only)
        var closedStatuses = new[] { "Success", "Failed" };

        var allFollowUps = await _dbContext.FollowUps
            .Include(f => f.Guest)
            .Where(f => f.ManagerId == managerId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();

        // Group by guest, take latest per guest, filter out closed ones
        ActiveLeads = allFollowUps
            .GroupBy(f => f.GuestId)
            .Select(g => g.First()) // latest per guest
            .Where(f => !closedStatuses.Contains(f.FollowupStatus))
            .Select(f => new ActiveLeadViewModel
            {
                GuestId = f.GuestId,
                FollowUpId = f.Id,
                GuestName = f.Guest?.Name ?? "—",
                Phone = f.Guest?.Mobile ?? "—",
                FollowupStatus = f.FollowupStatus,
                FollowupDate = f.FollowupDate,
                LastUpdated = f.UpdatedAt
            })
            .OrderByDescending(l => l.LastUpdated)
            .ToList();

        ActiveLeadsCount = ActiveLeads.Count;

        // Find guests initiated by this manager that have NO follow-up (incomplete leads)
        var allManagerGuests = await _guestRepository.Query()
            .Where(g => g.InitiatedByManagerId == managerId)
            .Include(g => g.FollowUps)
            .ToListAsync();

        IncompleteBookings = allManagerGuests
            .Where(g => !g.FollowUps.Any())
            .OrderByDescending(g => g.CreatedAt)
            .ToList();
    }

    public class ActiveLeadViewModel
    {
        public int GuestId { get; set; }
        public int FollowUpId { get; set; }
        public string GuestName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string FollowupStatus { get; set; } = "";
        public DateTime FollowupDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
