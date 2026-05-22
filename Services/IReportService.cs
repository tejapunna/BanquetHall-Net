using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public interface IReportService
{
    Task<RevenueReportDto> GetRevenueReportAsync(DateRangeFilter filter);
    Task<ManagerReportDto> GetManagerReportAsync(int managerId, DateRangeFilter filter);
    Task<IEnumerable<FollowUpMonitorDto>> GetFollowUpMonitorAsync(DateTime date);
    Task<decimal> CalculateConversionRateAsync(int managerId, DateTime from, DateTime to);
}
