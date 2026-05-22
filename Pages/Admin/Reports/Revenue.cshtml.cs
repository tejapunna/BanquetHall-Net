using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Services;
using BanquetHall.ViewModels;

namespace BanquetHall.Pages.Admin.Reports;

public class RevenueModel : PageModel
{
    private readonly IReportService _reportService;

    public RevenueModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true)]
    public DateRangeFilter Filter { get; set; } = new();

    public RevenueReportDto? Report { get; set; }

    public async Task OnGetAsync()
    {
        Report = await _reportService.GetRevenueReportAsync(Filter);
    }
}
