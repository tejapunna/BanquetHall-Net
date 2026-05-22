using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BanquetHall.Pages.Manager.FollowUps;

public class EditModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;

    public EditModel(BanquetHallDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Guest Guest { get; set; } = null!;
    public GuestFunction? LatestFunction { get; set; }
    public List<FollowUp> FollowUpHistory { get; set; } = new();
    public string? FunctionNameText { get; set; }
    public string? FunctionHallText { get; set; }
    public string? AssignedManagerName { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    [BindProperty]
    public FollowUpInput Input { get; set; } = new();

    // The guest ID (we navigate by guest, not by individual follow-up record)
    public int FollowUpId { get; set; }
    public int GuestId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // id = follow-up ID, use it to find the guest
        var followUp = await _dbContext.FollowUps.FindAsync(id);
        if (followUp == null) return NotFound();

        FollowUpId = id;
        GuestId = followUp.GuestId;
        await LoadGuestData(followUp.GuestId);

        // Get the latest follow-up for this guest to show current state
        var latest = FollowUpHistory.FirstOrDefault();

        Input = new FollowUpInput
        {
            FollowupStatus = latest?.FollowupStatus ?? "May Close",
            NextFollowupDate = DateTime.Today.AddDays(1),
            Remarks = ""
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var followUp = await _dbContext.FollowUps.FindAsync(id);
        if (followUp == null) return NotFound();

        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Create a NEW follow-up record (each follow-up = new row in DB)
        var newFollowUp = new FollowUp
        {
            GuestId = followUp.GuestId,
            ManagerId = managerId,
            Status = "Followup",
            FollowupStatus = Input.FollowupStatus,
            FollowupDate = Input.NextFollowupDate,
            Remarks = Input.Remarks,
            UpdatedAt = DateTime.Now
        };
        _dbContext.FollowUps.Add(newFollowUp);
        await _dbContext.SaveChangesAsync();

        SuccessMessage = "Follow-up saved successfully!";
        return RedirectToPage("/Manager/FollowUps/Edit", new { id });
    }

    private async Task LoadGuestData(int guestId)
    {
        Guest = (await _dbContext.Guests
            .Include(g => g.Functions)
            .FirstOrDefaultAsync(g => g.Id == guestId))!;

        LatestFunction = Guest?.Functions.OrderByDescending(f => f.FunctionDate).FirstOrDefault();

        if (LatestFunction != null)
        {
            if (LatestFunction.FunctionNameId.HasValue)
            {
                var fn = await _dbContext.FunctionNames.FindAsync(LatestFunction.FunctionNameId.Value);
                FunctionNameText = fn?.Name;
            }
            if (LatestFunction.FunctionHallId.HasValue)
            {
                var fh = await _dbContext.FunctionHalls.FindAsync(LatestFunction.FunctionHallId.Value);
                FunctionHallText = fh?.Name;
            }
            if (LatestFunction.AssignedManagerId.HasValue)
            {
                var am = await _dbContext.Users.FindAsync(LatestFunction.AssignedManagerId.Value);
                AssignedManagerName = am?.FullName;
            }
        }

        // Load ALL follow-ups for this guest as history
        FollowUpHistory = await _dbContext.FollowUps
            .Where(f => f.GuestId == guestId)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
    }

    public class FollowUpInput
    {
        [Required]
        public string FollowupStatus { get; set; } = "";

        [Required]
        public DateTime NextFollowupDate { get; set; } = DateTime.Today.AddDays(1);

        public string? Remarks { get; set; }
    }
}
