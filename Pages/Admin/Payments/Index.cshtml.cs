using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;

    public IndexModel(IPaymentRepository paymentRepository, IUserRepository userRepository)
    {
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
    }

    public List<Payment> PendingPayments { get; set; } = new();
    public List<Payment> CompletedPayments { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalPending { get; set; }
    public decimal TotalCollected { get; set; }

    public async Task OnGetAsync()
    {
        var allPayments = await _paymentRepository.Query()
            .Include(p => p.Guest)
            .Include(p => p.Function)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        PendingPayments = allPayments.Where(p => p.RemainingAmount > 0).ToList();
        CompletedPayments = allPayments.Where(p => p.RemainingAmount <= 0).ToList();

        TotalRevenue = allPayments.Sum(p => p.Amount);
        TotalPending = allPayments.Sum(p => p.RemainingAmount);
        TotalCollected = allPayments.Sum(p => p.AdvanceAmount);
    }
}
