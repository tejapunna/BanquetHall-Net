namespace BanquetHall.ViewModels;

public class ManagerReportDto
{
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int FollowUpCount { get; set; }
    public int DailyActivities { get; set; }
    public decimal ConversionRate { get; set; }
}
