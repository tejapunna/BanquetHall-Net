using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;
using BanquetHall.ViewModels;

namespace BanquetHall.Pages.Admin.ActivityLogs;

public class IndexModel : PageModel
{
    private readonly IActivityLogService _activityLogService;
    private readonly IUserRepository _userRepository;

    public IndexModel(IActivityLogService activityLogService, IUserRepository userRepository)
    {
        _activityLogService = activityLogService;
        _userRepository = userRepository;
    }

    [BindProperty(SupportsGet = true)]
    public int? UserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EntityType { get; set; }

    public IEnumerable<ActivityLog> Logs { get; set; } = Enumerable.Empty<ActivityLog>();
    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();

    public async Task OnGetAsync()
    {
        Users = await _userRepository.GetAllAsync();

        var filter = new ActivityLogFilter
        {
            UserId = UserId,
            ActionType = ActionType,
            EntityType = EntityType
        };

        Logs = (await _activityLogService.GetLogsAsync(filter))
            .OrderByDescending(l => l.Timestamp);
    }
}
