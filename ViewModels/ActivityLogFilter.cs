namespace BanquetHall.ViewModels;

public class ActivityLogFilter
{
    public int? UserId { get; set; }
    public string? ActionType { get; set; }
    public string? EntityType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
