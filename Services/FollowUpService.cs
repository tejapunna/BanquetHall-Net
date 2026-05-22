using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IActivityLogService _activityLogService;
    private readonly IUserRepository _userRepository;

    private static readonly string[] ValidStatuses = { "New", "In Progress", "Followup" };
    private static readonly string[] ValidFollowupStatuses = { "May Close", "May Not Close", "Success", "Failed" };

    public FollowUpService(
        IFollowUpRepository followUpRepository,
        IGuestRepository guestRepository,
        IActivityLogService activityLogService,
        IUserRepository userRepository)
    {
        _followUpRepository = followUpRepository;
        _guestRepository = guestRepository;
        _activityLogService = activityLogService;
        _userRepository = userRepository;
    }

    public async Task<FollowUp> CreateFollowUpAsync(FollowUpCreateDto dto, int managerId, int userId)
    {
        // Validate Guest exists
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest not found.");

        ValidateFollowUp(dto.Status, dto.FollowupStatus, dto.FollowupDate, dto.Remarks);

        var followUp = new FollowUp
        {
            GuestId = dto.GuestId,
            ManagerId = managerId,
            Status = dto.Status,
            FollowupStatus = dto.FollowupStatus,
            FollowupDate = dto.FollowupDate,
            Remarks = dto.Remarks,
            UpdatedAt = DateTime.Now
        };

        await _followUpRepository.AddAsync(followUp);
        await _followUpRepository.SaveChangesAsync();

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var username = user?.Username ?? userId.ToString();
            await _activityLogService.LogAsync(userId, username, "Create", "FollowUp", followUp.Id,
                $"Follow-up created for guest {dto.GuestId} with status '{dto.Status}'");
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return followUp;
    }

    public async Task<FollowUp> UpdateFollowUpAsync(int followUpId, FollowUpUpdateDto dto, int userId)
    {
        ValidateFollowUp(dto.Status, dto.FollowupStatus, dto.FollowupDate, dto.Remarks, validateDate: false);

        var followUp = await _followUpRepository.GetByIdAsync(followUpId);
        if (followUp == null)
            throw new InvalidOperationException("Follow-up not found.");

        followUp.Status = dto.Status;
        followUp.FollowupStatus = dto.FollowupStatus;
        followUp.FollowupDate = dto.FollowupDate;
        followUp.Remarks = dto.Remarks;
        followUp.UpdatedAt = DateTime.Now;

        _followUpRepository.Update(followUp);
        await _followUpRepository.SaveChangesAsync();

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var username = user?.Username ?? userId.ToString();
            await _activityLogService.LogAsync(userId, username, "Update", "FollowUp", followUp.Id,
                $"Follow-up {followUpId} updated to status '{dto.Status}'");
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return followUp;
    }

    public async Task<int> TransferLeadsAsync(int sourceManagerId, int targetManagerId, int adminUserId)
    {
        if (sourceManagerId == targetManagerId)
            throw new InvalidOperationException("Source and target manager cannot be the same.");

        var activeFollowUps = await _followUpRepository.GetActiveByManagerIdAsync(sourceManagerId);
        var followUpList = activeFollowUps.ToList();

        if (followUpList.Count == 0)
            return 0;

        foreach (var followUp in followUpList)
        {
            followUp.ManagerId = targetManagerId;
        }

        await _followUpRepository.SaveChangesAsync();

        try
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            var username = adminUser?.Username ?? adminUserId.ToString();
            await _activityLogService.LogTransferAsync(adminUserId, username, sourceManagerId, targetManagerId, followUpList.Count);
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return followUpList.Count;
    }

    private static void ValidateFollowUp(string status, string followupStatus, DateTime followupDate, string? remarks, bool validateDate = true)
    {
        if (!ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Status must be New, In Progress, or Followup.");

        if (!ValidFollowupStatuses.Contains(followupStatus, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Followup status must be May Close, May Not Close, Success, or Failed.");

        if (validateDate && followupDate.Date < DateTime.Today)
            throw new InvalidOperationException("Follow-up date cannot be in the past.");

        if (remarks != null && remarks.Length > 2000)
            throw new InvalidOperationException("Remarks cannot exceed 2000 characters.");
    }
}
