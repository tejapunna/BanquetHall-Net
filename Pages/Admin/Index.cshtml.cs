using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BanquetHall.Repositories;

namespace BanquetHall.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IGuestRepository _guestRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IPaymentRepository _paymentRepository;

    public IndexModel(
        IGuestRepository guestRepository,
        IFollowUpRepository followUpRepository,
        IPaymentRepository paymentRepository)
    {
        _guestRepository = guestRepository;
        _followUpRepository = followUpRepository;
        _paymentRepository = paymentRepository;
    }

    public int TotalGuests { get; set; }
    public int ActiveLeads { get; set; }
    public decimal TodayRevenue { get; set; }

    public async Task OnGetAsync()
    {
        TotalGuests = await _guestRepository.Query().CountAsync();

        var activeStatuses = new[] { "New", "In Progress", "Followup" };
        ActiveLeads = await _followUpRepository.Query()
            .Where(f => activeStatuses.Contains(f.Status))
            .CountAsync();

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        TodayRevenue = await _paymentRepository.Query()
            .Where(p => p.CreatedAt >= today && p.CreatedAt < tomorrow)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;
    }
}
