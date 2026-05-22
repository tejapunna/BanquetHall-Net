using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public class FunctionService : IFunctionService
{
    private readonly IGuestFunctionRepository _guestFunctionRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IActivityLogService _activityLogService;

    public FunctionService(
        IGuestFunctionRepository guestFunctionRepository,
        IGuestRepository guestRepository,
        IActivityLogService activityLogService)
    {
        _guestFunctionRepository = guestFunctionRepository;
        _guestRepository = guestRepository;
        _activityLogService = activityLogService;
    }

    public async Task<GuestFunction> CreateFunctionAsync(FunctionCreateDto dto, string managerName, int userId)
    {
        if (dto.FunctionDate.Date < DateTime.Today)
            throw new InvalidOperationException("Function date cannot be in the past.");

        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest not found.");

        var guestFunction = new GuestFunction
        {
            GuestId = dto.GuestId,
            FunctionDate = dto.FunctionDate,
            FunctionType = "", // Will be resolved from FunctionNameId
            FunctionNameId = dto.FunctionNameId,
            MealType = dto.MealType,
            MealPlan = dto.MealPlan,
            NoOfPacks = dto.NoOfPacks,
            GuaranteedPacks = dto.GuaranteedPacks,
            SpecialInstruction = dto.SpecialInstruction,
            AssignedManagerId = dto.AssignedManagerId,
            FunctionHallId = dto.FunctionHallId,
            InitiatedBy = managerName,
            CreatedAt = DateTime.Now
        };

        await _guestFunctionRepository.AddAsync(guestFunction);
        await _guestFunctionRepository.SaveChangesAsync();

        try
        {
            await _activityLogService.LogAsync(userId, managerName, "Create", "GuestFunction", guestFunction.Id,
                $"Lead function created for guest {dto.GuestId}");
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return guestFunction;
    }

    public async Task<IEnumerable<GuestFunction>> GetFunctionsByGuestAsync(int guestId)
    {
        return await _guestFunctionRepository.GetByGuestIdAsync(guestId);
    }
}
