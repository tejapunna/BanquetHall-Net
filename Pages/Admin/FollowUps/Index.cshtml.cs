using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.FollowUps;

public class IndexModel : PageModel
{
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IUserRepository _userRepository;

    public IndexModel(IFollowUpRepository followUpRepository, IUserRepository userRepository)
    {
        _followUpRepository = followUpRepository;
        _userRepository = userRepository;
    }

    [BindProperty(SupportsGet = true)]
    public int? ManagerId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FollowupStatusFilter { get; set; }

    public List<FollowUp> FollowUps { get; set; } = new();
    public List<User> Managers { get; set; } = new();

    public async Task OnGetAsync()
    {
        Managers = (await _userRepository.GetByRoleAsync("Manager")).ToList();

        IQueryable<FollowUp> query = _followUpRepository.Query()
            .Include(f => f.Guest)
            .Include(f => f.Manager);

        if (ManagerId.HasValue)
        {
            query = query.Where(f => f.ManagerId == ManagerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query = query.Where(f => f.Status == StatusFilter);
        }

        if (!string.IsNullOrWhiteSpace(FollowupStatusFilter))
        {
            query = query.Where(f => f.FollowupStatus == FollowupStatusFilter);
        }

        FollowUps = await query.OrderByDescending(f => f.FollowupDate).ToListAsync();
    }
}
