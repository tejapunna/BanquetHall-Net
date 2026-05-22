using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Services;

public class ReportService : IReportService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IUserRepository _userRepository;

    public ReportService(
        IPaymentRepository paymentRepository,
        IFollowUpRepository followUpRepository,
        IGuestRepository guestRepository,
        IUserRepository userRepository)
    {
        _paymentRepository = paymentRepository;
        _followUpRepository = followUpRepository;
        _guestRepository = guestRepository;
        _userRepository = userRepository;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateRangeFilter filter)
    {
        var (from, to) = filter.GetDateRange();

        var payments = await _paymentRepository.GetByDateRangeAsync(from, to);
        var paymentList = payments.ToList();

        var dailyBreakdown = paymentList
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(p => p.Amount),
                BookingCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new RevenueReportDto
        {
            TotalRevenue = paymentList.Sum(p => p.Amount),
            TotalBookings = paymentList.Count,
            FromDate = from,
            ToDate = to,
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<ManagerReportDto> GetManagerReportAsync(int managerId, DateRangeFilter filter)
    {
        var (from, to) = filter.GetDateRange();

        var manager = await _userRepository.GetByIdAsync(managerId);
        var managerName = manager?.FullName ?? "Unknown";

        // Single efficient query: join payments with guests to get manager's revenue
        var totalRevenue = await _paymentRepository.Query()
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .Where(p => p.Guest.InitiatedByManagerId == managerId)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        // Get follow-up count in a single query
        var followUpCount = await _followUpRepository.Query()
            .Where(f => f.ManagerId == managerId)
            .CountAsync();

        // Daily activities (follow-ups updated in range) - single query
        var dailyActivities = await _followUpRepository.Query()
            .Where(f => f.ManagerId == managerId && f.UpdatedAt >= from && f.UpdatedAt <= to)
            .CountAsync();

        // Conversion rate
        var conversionRate = await CalculateConversionRateAsync(managerId, from, to);

        return new ManagerReportDto
        {
            ManagerId = managerId,
            ManagerName = managerName,
            TotalRevenue = totalRevenue,
            FollowUpCount = followUpCount,
            DailyActivities = dailyActivities,
            ConversionRate = conversionRate
        };
    }

    public async Task<IEnumerable<FollowUpMonitorDto>> GetFollowUpMonitorAsync(DateTime date)
    {
        var managers = await _userRepository.GetByRoleAsync("Manager");
        var managerIds = managers.Select(m => m.Id).ToList();

        // Batch load all follow-ups for all managers in a single query
        var allFollowUps = await _followUpRepository.Query()
            .Where(f => managerIds.Contains(f.ManagerId))
            .Include(f => f.Guest)
            .ToListAsync();

        var upcomingDate = date.AddDays(7);
        var results = new List<FollowUpMonitorDto>();

        foreach (var manager in managers)
        {
            var managerFollowUps = allFollowUps
                .Where(f => f.ManagerId == manager.Id)
                .ToList();

            // Follow-ups updated on the given date
            var updatedToday = managerFollowUps
                .Where(f => f.UpdatedAt.Date == date.Date)
                .ToList();

            // Guests contacted - use already-loaded Guest navigation property
            var contactedGuests = updatedToday
                .Where(f => f.Guest != null)
                .Select(f => new ContactedGuestDto
                {
                    GuestId = f.Guest.Id,
                    GuestName = f.Guest.Name,
                    FollowupStatus = f.FollowupStatus
                })
                .ToList();

            // Upcoming follow-ups within 7 days
            var upcomingDtos = managerFollowUps
                .Where(f => f.FollowupDate.Date >= date.Date && f.FollowupDate.Date <= upcomingDate.Date)
                .Where(f => f.Guest != null)
                .Select(f => new UpcomingFollowUpDto
                {
                    FollowUpId = f.Id,
                    GuestId = f.Guest.Id,
                    GuestName = f.Guest.Name,
                    FollowupDate = f.FollowupDate,
                    Status = f.Status
                })
                .ToList();

            results.Add(new FollowUpMonitorDto
            {
                ManagerId = manager.Id,
                ManagerName = manager.FullName,
                FollowUpsUpdatedToday = updatedToday.Count,
                ContactedGuests = contactedGuests,
                UpcomingFollowUps = upcomingDtos
            });
        }

        return results;
    }

    public async Task<decimal> CalculateConversionRateAsync(int managerId, DateTime from, DateTime to)
    {
        // Single query to get counts
        var inRangeCount = await _followUpRepository.Query()
            .Where(f => f.ManagerId == managerId && f.UpdatedAt >= from && f.UpdatedAt <= to)
            .CountAsync();

        if (inRangeCount == 0)
            return 0;

        var successCount = await _followUpRepository.Query()
            .Where(f => f.ManagerId == managerId && f.UpdatedAt >= from && f.UpdatedAt <= to && f.FollowupStatus == "Success")
            .CountAsync();

        return (decimal)successCount / inRangeCount;
    }
}
