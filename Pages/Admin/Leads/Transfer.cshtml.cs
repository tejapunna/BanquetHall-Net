using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Leads;

public class TransferModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IFollowUpService _followUpService;
    private readonly IActivityLogService _activityLogService;

    public TransferModel(
        IUserRepository userRepository,
        IFollowUpRepository followUpRepository,
        IFollowUpService followUpService,
        IActivityLogService activityLogService)
    {
        _userRepository = userRepository;
        _followUpRepository = followUpRepository;
        _followUpService = followUpService;
        _activityLogService = activityLogService;
    }

    public IEnumerable<User> Managers { get; set; } = Enumerable.Empty<User>();
    public List<FollowUp> SourceLeads { get; set; } = new();

    [BindProperty]
    public int SourceManagerId { get; set; }

    [BindProperty]
    public int TargetManagerId { get; set; }

    [BindProperty]
    public List<int> SelectedLeadIds { get; set; } = new();

    [BindProperty]
    public bool TransferAll { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int? sourceId)
    {
        Managers = await _userRepository.GetByRoleAsync("Manager");

        if (sourceId.HasValue)
        {
            SourceManagerId = sourceId.Value;
            var leads = await _followUpRepository.GetActiveByManagerIdAsync(sourceId.Value);
            SourceLeads = leads.ToList();
        }
    }

    public async Task<IActionResult> OnPostLoadLeadsAsync()
    {
        Managers = await _userRepository.GetByRoleAsync("Manager");

        if (SourceManagerId > 0)
        {
            var leads = await _followUpRepository.GetActiveByManagerIdAsync(SourceManagerId);
            SourceLeads = leads.ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostTransferAsync()
    {
        Managers = await _userRepository.GetByRoleAsync("Manager");

        if (SourceManagerId == 0 || TargetManagerId == 0)
        {
            ErrorMessage = "Please select both source and target managers.";
            return RedirectToPage(new { sourceId = SourceManagerId });
        }

        if (SourceManagerId == TargetManagerId)
        {
            ErrorMessage = "Source and target managers cannot be the same.";
            return RedirectToPage(new { sourceId = SourceManagerId });
        }

        var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (TransferAll)
        {
            // Bulk transfer all active leads
            var count = await _followUpService.TransferLeadsAsync(SourceManagerId, TargetManagerId, adminUserId);
            if (count == 0)
                ErrorMessage = "No active leads found for the source manager.";
            else
                SuccessMessage = $"Successfully transferred {count} lead(s).";
        }
        else
        {
            // Transfer only selected leads
            if (!SelectedLeadIds.Any())
            {
                ErrorMessage = "Please select at least one lead to transfer.";
                return RedirectToPage(new { sourceId = SourceManagerId });
            }

            int transferred = 0;
            foreach (var leadId in SelectedLeadIds)
            {
                var followUp = await _followUpRepository.GetByIdAsync(leadId);
                if (followUp != null && followUp.ManagerId == SourceManagerId)
                {
                    followUp.ManagerId = TargetManagerId;
                    transferred++;
                }
            }
            await _followUpRepository.SaveChangesAsync();

            if (transferred > 0)
            {
                try
                {
                    var adminUser = await _userRepository.GetByIdAsync(adminUserId);
                    var username = adminUser?.Username ?? adminUserId.ToString();
                    await _activityLogService.LogTransferAsync(adminUserId, username, SourceManagerId, TargetManagerId, transferred);
                }
                catch { }
                SuccessMessage = $"Successfully transferred {transferred} selected lead(s).";
            }
            else
            {
                ErrorMessage = "No leads were transferred.";
            }
        }

        return RedirectToPage(new { sourceId = SourceManagerId });
    }
}
