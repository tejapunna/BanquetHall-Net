using BanquetHall.Models;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public interface IActivityLogService
{
    Task LogAsync(int userId, string username, string actionType, string entityType, int entityId, string? details = null);
    Task LogTransferAsync(int userId, string username, int sourceManagerId, int targetManagerId, int leadsTransferred);
    Task<IEnumerable<ActivityLog>> GetLogsAsync(ActivityLogFilter filter);
}
