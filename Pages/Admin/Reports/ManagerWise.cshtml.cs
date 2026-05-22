using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Repositories;
using BanquetHall.Services;
using BanquetHall.ViewModels;

namespace BanquetHall.Pages.Admin.Reports;

public class ManagerWiseModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly IUserRepository _userRepository;

    public ManagerWiseModel(IReportService reportService, IUserRepository userRepository)
    {
        _reportService = reportService;
        _userRepository = userRepository;
    }

    [BindProperty(SupportsGet = true)]
    public DateRangeFilter Filter { get; set; } = new();

    public List<ManagerReportDto> ManagerReports { get; set; } = new();

    public async Task OnGetAsync()
    {
        var managers = await _userRepository.GetByRoleAsync("Manager");

        foreach (var manager in managers)
        {
            var report = await _reportService.GetManagerReportAsync(manager.Id, Filter);
            ManagerReports.Add(report);
        }
    }
}
