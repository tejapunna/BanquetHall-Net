using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IGuestFunctionRepository _guestFunctionRepository;
    private readonly IActivityLogService _activityLogService;
    private readonly IUserRepository _userRepository;

    private static readonly string[] ValidPaymentTypes = { "Cash", "UPI", "Card" };

    public PaymentService(
        IPaymentRepository paymentRepository,
        IGuestRepository guestRepository,
        IGuestFunctionRepository guestFunctionRepository,
        IActivityLogService activityLogService,
        IUserRepository userRepository)
    {
        _paymentRepository = paymentRepository;
        _guestRepository = guestRepository;
        _guestFunctionRepository = guestFunctionRepository;
        _activityLogService = activityLogService;
        _userRepository = userRepository;
    }

    public async Task<Payment> CreatePaymentAsync(PaymentCreateDto dto, int userId)
    {
        // Validate Guest exists
        var guest = await _guestRepository.GetByIdAsync(dto.GuestId);
        if (guest == null)
            throw new InvalidOperationException("Guest not found.");

        // Validate Function exists
        var function = await _guestFunctionRepository.GetByIdAsync(dto.FunctionId);
        if (function == null)
            throw new InvalidOperationException("Function not found.");

        // Validate PaymentType
        if (!ValidPaymentTypes.Contains(dto.PaymentType, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Payment type must be Cash, UPI, or Card.");

        // Calculate Amount
        decimal amount = dto.GuaranteedPacks * dto.PricePerPack;

        // Handle AdvancePaid logic
        decimal advanceAmount = 0;
        if (dto.AdvancePaid)
        {
            if (dto.AdvanceAmount <= 0)
                throw new InvalidOperationException("Advance amount must be greater than 0 when advance is paid.");
            if (dto.AdvanceAmount > amount)
                throw new InvalidOperationException("Advance amount cannot exceed total amount.");
            advanceAmount = dto.AdvanceAmount;
        }

        decimal remainingAmount = amount - advanceAmount;

        var payment = new Payment
        {
            GuestId = dto.GuestId,
            FunctionId = dto.FunctionId,
            PaymentType = dto.PaymentType,
            NoOfPacks = dto.NoOfPacks,
            GuaranteedPacks = dto.GuaranteedPacks,
            PricePerPack = dto.PricePerPack,
            Amount = amount,
            AdvancePaid = dto.AdvancePaid,
            AdvanceAmount = advanceAmount,
            RemainingAmount = remainingAmount,
            CreatedAt = DateTime.Now
        };

        await _paymentRepository.AddAsync(payment);
        await _paymentRepository.SaveChangesAsync();

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var username = user?.Username ?? userId.ToString();
            await _activityLogService.LogAsync(userId, username, "Create", "Payment", payment.Id,
                $"Payment of {amount:C} created for guest {dto.GuestId}");
        }
        catch
        {
            // Activity logging failure should not break the main operation
        }

        return payment;
    }

    public Task<PaymentSummaryDto> CalculatePaymentAsync(int guaranteedPacks, decimal pricePerPack, decimal advanceAmount)
    {
        decimal amount = guaranteedPacks * pricePerPack;
        decimal remaining = amount - advanceAmount;

        var summary = new PaymentSummaryDto
        {
            Amount = amount,
            AdvanceAmount = advanceAmount,
            RemainingAmount = remaining
        };

        return Task.FromResult(summary);
    }

    public async Task<Payment> CollectRemainingPaymentAsync(int paymentId, decimal amountCollected, string paymentType, int userId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            throw new InvalidOperationException("Payment not found.");

        if (amountCollected <= 0)
            throw new InvalidOperationException("Amount must be greater than 0.");

        if (amountCollected > payment.RemainingAmount)
            throw new InvalidOperationException("Amount cannot exceed remaining balance.");

        payment.AdvanceAmount += amountCollected;
        payment.RemainingAmount -= amountCollected;
        payment.AdvancePaid = true;

        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync();

        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var username = user?.Username ?? userId.ToString();
            await _activityLogService.LogAsync(userId, username, "Update", "Payment", payment.Id,
                $"Collected ₹{amountCollected} for payment {paymentId}. Remaining: ₹{payment.RemainingAmount}");
        }
        catch { }

        return payment;
    }
}
