using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;
using BanquetHall.Models;

namespace BanquetHall.Repositories;

public class FollowUpRepository : Repository<FollowUp>, IFollowUpRepository
{
    public FollowUpRepository(BanquetHallDbContext context) : base(context) { }

    public async Task<IEnumerable<FollowUp>> GetByManagerIdAsync(int managerId)
    {
        return await _dbSet
            .Where(f => f.ManagerId == managerId)
            .Include(f => f.Guest)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FollowUp>> GetActiveByManagerIdAsync(int managerId)
    {
        var activeStatuses = new[] { "New", "In Progress", "Followup" };
        var closedFollowupStatuses = new[] { "Success", "Failed" };
        return await _dbSet
            .Where(f => f.ManagerId == managerId 
                && activeStatuses.Contains(f.Status)
                && !closedFollowupStatuses.Contains(f.FollowupStatus))
            .Include(f => f.Guest)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
    }

    public async Task<FollowUp?> GetLatestByGuestIdAsync(int guestId)
    {
        return await _dbSet
            .Where(f => f.GuestId == guestId)
            .Include(f => f.Manager)
            .OrderByDescending(f => f.UpdatedAt)
            .FirstOrDefaultAsync();
    }
}
