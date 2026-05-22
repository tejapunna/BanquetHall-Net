using System.Security.Claims;
using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Reports;

public class MyPerformanceModel : PageModel
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IGuestFunctionRepository _guestFunctionRepository;

    public MyPerformanceModel(
        IFollowUpRepository followUpRepository,
        IPaymentRepository paymentRepository,
        IGuestRepository guestRepository,
        IGuestFunctionRepository guestFunctionRepository)
    {
        _followUpRepository = followUpRepository;
        _paymentRepository = paymentRepository;
        _guestRepository = guestRepository;
        _guestFunctionRepository = guestFunctionRepository;
    }

    public int FollowUpCount { get; set; }
    public int SuccessfulLeads { get; set; }
    public int FailedLeads { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingAmount { get; set; }
    public List<GuestFunction> BookingHistory { get; set; } = new();
    public List<FollowUp> FollowUpHistory { get; set; } = new();

    public async Task OnGetAsync()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Follow-up stats
        var followUps = await _followUpRepository.GetByManagerIdAsync(managerId);
        var followUpList = followUps.ToList();
        FollowUpCount = followUpList.Count;
        SuccessfulLeads = followUpList.Count(f => f.FollowupStatus == "Success");
        FailedLeads = followUpList.Count(f => f.FollowupStatus == "Failed");
        FollowUpHistory = followUpList;

        // Payment stats
        var guestIds = followUpList.Select(f => f.GuestId).Distinct().ToHashSet();
        var payments = await _paymentRepository.Query()
            .Where(p => guestIds.Contains(p.GuestId))
            .ToListAsync();
        TotalRevenue = payments.Sum(p => p.Amount);
        PendingAmount = payments.Sum(p => p.RemainingAmount);

        // Booking history - functions for guests managed by this manager
        BookingHistory = await _guestFunctionRepository.Query()
            .Include(gf => gf.Guest)
            .Where(gf => guestIds.Contains(gf.GuestId))
            .OrderByDescending(gf => gf.FunctionDate)
            .ToListAsync();
    }
}
