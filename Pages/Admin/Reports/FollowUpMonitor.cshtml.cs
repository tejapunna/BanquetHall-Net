using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Services;
using BanquetHall.ViewModels;

namespace BanquetHall.Pages.Admin.Reports;

public class FollowUpMonitorModel : PageModel
{
    private readonly IReportService _reportService;

    public FollowUpMonitorModel(IReportService reportService)
    {
        _reportService = reportService;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime MonitorDate { get; set; } = DateTime.Today;

    public IEnumerable<FollowUpMonitorDto> MonitorData { get; set; } = Enumerable.Empty<FollowUpMonitorDto>();

    public async Task OnGetAsync()
    {
        MonitorData = await _reportService.GetFollowUpMonitorAsync(MonitorDate);
    }
}
