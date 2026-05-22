using System.Security.Claims;
using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFollowUpRepository _followUpRepository;

    public IndexModel(IPaymentRepository paymentRepository, IFollowUpRepository followUpRepository)
    {
        _paymentRepository = paymentRepository;
        _followUpRepository = followUpRepository;
    }

    public List<PaymentWithStatus> PendingPayments { get; set; } = new();
    public List<PaymentWithStatus> CompletedPayments { get; set; } = new();
    public List<PaymentWithStatus> CancelledPayments { get; set; } = new();
    public decimal TotalPending { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalRefunded { get; set; }

    public async Task OnGetAsync()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var followUps = await _followUpRepository.GetByManagerIdAsync(managerId);
        var guestIds = followUps.Select(f => f.GuestId).Distinct().ToHashSet();

        var allPayments = await _paymentRepository.Query()
            .Include(p => p.Guest)
            .Include(p => p.Function)
            .Where(p => guestIds.Contains(p.GuestId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Get latest follow-up status per guest
        var latestFollowUps = followUps
            .GroupBy(f => f.GuestId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(f => f.UpdatedAt).First());

        var paymentsWithStatus = allPayments.Select(p =>
        {
            var latestFu = latestFollowUps.GetValueOrDefault(p.GuestId);
            return new PaymentWithStatus
            {
                Payment = p,
                FollowupStatus = latestFu?.FollowupStatus ?? "Unknown",
                CanCollect = latestFu?.FollowupStatus == "Success" && p.RemainingAmount > 0,
                IsCancelled = latestFu?.FollowupStatus == "Failed"
            };
        }).ToList();

        PendingPayments = paymentsWithStatus.Where(p => p.Payment.RemainingAmount > 0 && !p.IsCancelled).ToList();
        CompletedPayments = paymentsWithStatus.Where(p => p.Payment.RemainingAmount <= 0 && !p.IsCancelled).ToList();
        CancelledPayments = paymentsWithStatus.Where(p => p.IsCancelled).ToList();

        TotalPending = PendingPayments.Sum(p => p.Payment.RemainingAmount);
        TotalCollected = allPayments.Sum(p => p.AdvanceAmount);
        TotalRefunded = 0; // Tracked via activity logs
    }

    public class PaymentWithStatus
    {
        public Payment Payment { get; set; } = null!;
        public string FollowupStatus { get; set; } = string.Empty;
        public bool CanCollect { get; set; }
        public bool IsCancelled { get; set; }
    }
}
