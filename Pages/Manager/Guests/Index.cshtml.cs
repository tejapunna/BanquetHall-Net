using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Guests;

public class IndexModel : PageModel
{
    private readonly IGuestRepository _guestRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly BanquetHallDbContext _dbContext;

    public IndexModel(IGuestRepository guestRepository, IFollowUpRepository followUpRepository, BanquetHallDbContext dbContext)
    {
        _guestRepository = guestRepository;
        _followUpRepository = followUpRepository;
        _dbContext = dbContext;
    }

    public List<Guest> Guests { get; set; } = new();

    public async Task OnGetAsync()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get guest IDs assigned to this manager via function
        var assignedGuestIds = await _dbContext.GuestFunctions
            .Where(f => f.AssignedManagerId == managerId)
            .Select(f => f.GuestId)
            .Distinct()
            .ToListAsync();

        // Get active follow-up guest IDs for this manager
        var activeFollowUps = await _followUpRepository.GetActiveByManagerIdAsync(managerId);
        var followUpGuestIds = activeFollowUps.Select(f => f.GuestId).Distinct().ToHashSet();

        // Combine all: initiated by OR assigned via function OR has active follow-up
        var allGuestIds = new HashSet<int>(assignedGuestIds);
        allGuestIds.UnionWith(followUpGuestIds);

        var guests = await _guestRepository.Query()
            .Where(g => g.InitiatedByManagerId == managerId || allGuestIds.Contains(g.Id))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        Guests = guests;
    }
}
