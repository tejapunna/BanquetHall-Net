using BanquetHall.Models;

namespace BanquetHall.Repositories;

public interface IActivityLogRepository : IRepository<ActivityLog>
{
    Task<IEnumerable<ActivityLog>> GetFilteredAsync(int? userId, string? actionType, string? entityType, DateTime? from, DateTime? to);
}
