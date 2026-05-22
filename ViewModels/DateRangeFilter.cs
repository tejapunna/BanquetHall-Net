namespace BanquetHall.ViewModels;

public class DateRangeFilter
{
    public string FilterType { get; set; } = "Today";
    public DateTime? ExactDate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public (DateTime From, DateTime To) GetDateRange()
    {
        var today = DateTime.Today;
        return FilterType switch
        {
            "Today" => (today, today.AddDays(1).AddTicks(-1)),
            "ThisWeek" => (today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday), today.AddDays(1).AddTicks(-1)),
            "ThisMonth" => (new DateTime(today.Year, today.Month, 1), today.AddDays(1).AddTicks(-1)),
            "FinancialYear" => (today.Month >= 4 ? new DateTime(today.Year, 4, 1) : new DateTime(today.Year - 1, 4, 1), today.AddDays(1).AddTicks(-1)),
            "ExactDate" => (ExactDate ?? today, (ExactDate ?? today).AddDays(1).AddTicks(-1)),
            "Custom" => (FromDate ?? today, (ToDate ?? today).AddDays(1).AddTicks(-1)),
            _ => (today, today.AddDays(1).AddTicks(-1))
        };
    }
}
