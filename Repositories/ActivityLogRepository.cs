using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;
using BanquetHall.Models;

namespace BanquetHall.Repositories;

public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
{
    public ActivityLogRepository(BanquetHallDbContext context) : base(context) { }

    public async Task<IEnumerable<ActivityLog>> GetFilteredAsync(int? userId, string? actionType, string? entityType, DateTime? from, DateTime? to)
    {
        var query = _dbSet.AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(actionType))
            query = query.Where(a => a.ActionType == actionType);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }
}
