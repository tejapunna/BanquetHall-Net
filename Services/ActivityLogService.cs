using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogService(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    public async Task LogAsync(int userId, string username, string actionType, string entityType, int entityId, string? details = null)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Username = username,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.Now
        };

        await _activityLogRepository.AddAsync(log);
        await _activityLogRepository.SaveChangesAsync();
    }

    public async Task LogTransferAsync(int userId, string username, int sourceManagerId, int targetManagerId, int leadsTransferred)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Username = username,
            ActionType = "Transfer",
            EntityType = "FollowUp",
            EntityId = null,
            Details = $"Transferred {leadsTransferred} leads from manager {sourceManagerId} to manager {targetManagerId}",
            Timestamp = DateTime.Now
        };

        await _activityLogRepository.AddAsync(log);
        await _activityLogRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsAsync(ActivityLogFilter filter)
    {
        return await _activityLogRepository.GetFilteredAsync(
            filter.UserId,
            filter.ActionType,
            filter.EntityType,
            filter.FromDate,
            filter.ToDate);
    }
}
