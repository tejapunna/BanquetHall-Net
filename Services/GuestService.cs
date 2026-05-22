using BanquetHall.Helpers;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Services;

public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;
    private readonly IGuestFunctionRepository _guestFunctionRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IActivityLogService _activityLogService;
    private readonly IUserRepository _userRepository;

    public GuestService(
        IGuestRepository guestRepository,
        IGuestFunctionRepository guestFunctionRepository,
        IFollowUpRepository followUpRepository,
        IActivityLogService activityLogService,
        IUserRepository userRepository)
    {
        _guestRepository = guestRepository;
        _guestFunctionRepository = guestFunctionRepository;
        _followUpRepository = followUpRepository;
        _activityLogService = activityLogService;
        _userRepository = userRepository;
    }

    public async Task<Guest> CreateGuestAsync(GuestCreateDto dto, int managerId)
    {
        var guest = new Guest
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Name = $"{dto.FirstName} {dto.LastName}",
            Mobile = dto.Mobile,
            Email = dto.Email,
            Village = dto.Village,
            Mandal = dto.Mandal,
            GuestAadhaar = dto.GuestAadhaar,
            GuestPan = dto.GuestPan,
            Remarks = dto.Remarks,
            ReferredByName = dto.ReferredByName,
            ReferredByPhone = dto.ReferredByPhone,
            Status = "New",
            CreatedAt = DateTime.Now,
            InitiatedByManagerId = managerId
        };

        await _guestRepository.AddAsync(guest);
        await _guestRepository.SaveChangesAsync();

        try
        {
            var manager = await _userRepository.GetByIdAsync(managerId);
            var username = manager?.Username ?? managerId.ToString();
            await _activityLogService.LogAsync(managerId, username, "Create", "Guest", guest.Id, $"Guest '{guest.Name}' created");
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return guest;
    }

    public async Task<Guest> UpdateGuestAsync(int guestId, GuestUpdateDto dto, int userId)
    {
        var guest = await _guestRepository.GetByIdAsync(guestId);
        if (guest == null)
            throw new InvalidOperationException("Guest not found.");

        // Validate edit permission: user must be the initiating manager or have an active follow-up
        var hasPermission = guest.InitiatedByManagerId == userId;
        if (!hasPermission)
        {
            var activeFollowUps = await _followUpRepository.GetActiveByManagerIdAsync(userId);
            hasPermission = activeFollowUps.Any(f => f.GuestId == guestId);
        }

        if (!hasPermission)
            throw new UnauthorizedAccessException("You do not have permission to edit this guest.");

        guest.Name = dto.Name;
        guest.Mobile = dto.Mobile;
        guest.Email = dto.Email;
        guest.ReferredByName = dto.ReferredByName;
        guest.ReferredByPhone = dto.ReferredByPhone;
        guest.Status = dto.Status;

        _guestRepository.Update(guest);
        await _guestRepository.SaveChangesAsync();

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var username = user?.Username ?? userId.ToString();
            await _activityLogService.LogAsync(userId, username, "Update", "Guest", guest.Id, $"Guest '{guest.Name}' updated");
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return guest;
    }

    public async Task<IEnumerable<GuestSearchResultDto>> SearchGuestsAsync(string term)
    {
        var guests = await _guestRepository.SearchAsync(term);

        var results = new List<GuestSearchResultDto>();

        foreach (var guest in guests)
        {
            var latestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(guest.Id);

            results.Add(new GuestSearchResultDto
            {
                Id = guest.Id,
                Name = guest.Name,
                Mobile = guest.Mobile,
                Email = guest.Email,
                Status = guest.Status,
                CurrentManager = latestFollowUp?.Manager?.FullName,
                InitiatedByManager = null
            });
        }

        // Also check by Aadhaar
        if (!string.IsNullOrWhiteSpace(term) && term.Length >= 3)
        {
            var functionByAadhaar = await _guestFunctionRepository.FindByAadhaarAsync(term);
            if (functionByAadhaar != null && !results.Any(r => r.Id == functionByAadhaar.GuestId))
            {
                var guest = await _guestRepository.GetByIdAsync(functionByAadhaar.GuestId);
                if (guest != null)
                {
                    var latestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(guest.Id);
                    results.Add(new GuestSearchResultDto
                    {
                        Id = guest.Id,
                        Name = guest.Name,
                        Mobile = guest.Mobile,
                        Email = guest.Email,
                        Status = guest.Status,
                        CurrentManager = latestFollowUp?.Manager?.FullName,
                        InitiatedByManager = null
                    });
                }
            }
        }

        return results;
    }

    public async Task<GuestDetailDto?> GetGuestDetailAsync(int guestId, string userRole)
    {
        var guest = await _guestRepository.Query()
            .Include(g => g.Functions)
            .Include(g => g.FollowUps)
                .ThenInclude(f => f.Manager)
            .FirstOrDefaultAsync(g => g.Id == guestId);

        if (guest == null)
            return null;

        var latestFollowUp = guest.FollowUps
            .OrderByDescending(f => f.UpdatedAt)
            .FirstOrDefault();

        // Resolve initiating manager name
        string? initiatedByManager = null;
        if (guest.InitiatedByManagerId.HasValue)
        {
            var manager = await _userRepository.GetByIdAsync(guest.InitiatedByManagerId.Value);
            initiatedByManager = manager?.FullName;
        }

        return new GuestDetailDto
        {
            Id = guest.Id,
            Name = guest.Name,
            Mobile = guest.Mobile,
            Email = guest.Email,
            ReferredByName = guest.ReferredByName,
            ReferredByPhone = guest.ReferredByPhone,
            Status = guest.Status,
            InitiatedByManager = initiatedByManager,
            CurrentFollowUpManager = latestFollowUp?.Manager?.FullName,
            Functions = guest.Functions.Select(f => new FunctionHistoryDto
            {
                Id = f.Id,
                FunctionDate = f.FunctionDate,
                FunctionType = f.FunctionType,
                MealPlan = f.MealPlan,
                GuestAddress = f.GuestAddress,
                GuestAadhaar = AadhaarHelper.MaskAadhaar(f.GuestAadhaar, userRole),
                InitiatedBy = f.InitiatedBy
            }).ToList()
        };
    }
}
