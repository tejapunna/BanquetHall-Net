using System.Security.Claims;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Manager.Payments;

public class CollectModel : PageModel
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentService _paymentService;
    private readonly IFollowUpRepository _followUpRepository;

    public CollectModel(IPaymentRepository paymentRepository, IPaymentService paymentService, IFollowUpRepository followUpRepository)
    {
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _followUpRepository = followUpRepository;
    }

    public Payment? PaymentRecord { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public decimal AmountCollected { get; set; }

    [BindProperty]
    public string PaymentType { get; set; } = "Cash";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        PaymentRecord = await _paymentRepository.GetByIdAsync(id);
        if (PaymentRecord == null)
            return NotFound();

        // Check if collection is allowed (only when follow-up status is Success)
        var latestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(PaymentRecord.GuestId);
        if (latestFollowUp == null || latestFollowUp.FollowupStatus != "Success")
        {
            ErrorMessage = "Payment collection is only allowed when the follow-up status is 'Success'. Current status: " + (latestFollowUp?.FollowupStatus ?? "None");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        PaymentRecord = await _paymentRepository.GetByIdAsync(id);
        if (PaymentRecord == null)
            return NotFound();

        // Validate follow-up status before processing
        var latestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(PaymentRecord.GuestId);
        if (latestFollowUp == null || latestFollowUp.FollowupStatus != "Success")
        {
            ErrorMessage = "Payment collection is only allowed when the follow-up status is 'Success'.";
            return Page();
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _paymentService.CollectRemainingPaymentAsync(id, AmountCollected, PaymentType, userId);
            SuccessMessage = $"Successfully collected ₹{AmountCollected:N2}. New remaining: ₹{(PaymentRecord.RemainingAmount - AmountCollected):N2}";
            PaymentRecord = await _paymentRepository.GetByIdAsync(id); // Refresh
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
