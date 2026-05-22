using System.Security.Claims;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Payments;

public class CancelModel : PageModel
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IActivityLogService _activityLogService;
    private readonly IUserRepository _userRepository;

    public CancelModel(
        IPaymentRepository paymentRepository,
        IFollowUpRepository followUpRepository,
        IActivityLogService activityLogService,
        IUserRepository userRepository)
    {
        _paymentRepository = paymentRepository;
        _followUpRepository = followUpRepository;
        _activityLogService = activityLogService;
        _userRepository = userRepository;
    }

    public Payment? PaymentRecord { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public decimal RefundAmount { get; set; }

    [BindProperty]
    public string CancellationReason { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        PaymentRecord = await _paymentRepository.Query()
            .Include(p => p.Guest)
            .Include(p => p.Function)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (PaymentRecord == null)
            return NotFound();

        // Default refund to the advance amount paid
        RefundAmount = PaymentRecord.AdvanceAmount;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        PaymentRecord = await _paymentRepository.Query()
            .Include(p => p.Guest)
            .Include(p => p.Function)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (PaymentRecord == null)
            return NotFound();

        if (RefundAmount < 0)
        {
            ErrorMessage = "Refund amount cannot be negative.";
            return Page();
        }

        if (RefundAmount > PaymentRecord.AdvanceAmount)
        {
            ErrorMessage = "Refund amount cannot exceed the advance paid.";
            return Page();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Update the payment: set remaining to 0 (cancelled - no more collection expected)
        PaymentRecord.RemainingAmount = 0;
        _paymentRepository.Update(PaymentRecord);
        await _paymentRepository.SaveChangesAsync();

        // Update the latest follow-up for this guest to "Failed"
        var latestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(PaymentRecord.GuestId);
        if (latestFollowUp != null)
        {
            latestFollowUp.FollowupStatus = "Failed";
            latestFollowUp.Remarks = $"Booking cancelled. Reason: {CancellationReason}. Refund: ₹{RefundAmount:N2}";
            latestFollowUp.UpdatedAt = DateTime.Now;
            _followUpRepository.Update(latestFollowUp);
            await _followUpRepository.SaveChangesAsync();
        }

        // Log the cancellation
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var username = user?.Username ?? userId.ToString();
            await _activityLogService.LogAsync(userId, username, "Update", "Payment", PaymentRecord.Id,
                $"Booking cancelled for guest {PaymentRecord.Guest?.Name}. Refund: ₹{RefundAmount:N2}. Reason: {CancellationReason}");
        }
        catch { }

        SuccessMessage = $"Booking cancelled successfully. Refund of ₹{RefundAmount:N2} recorded.";
        return Page();
    }
}
