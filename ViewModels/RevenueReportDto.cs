namespace BanquetHall.ViewModels;

public class RevenueReportDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}
