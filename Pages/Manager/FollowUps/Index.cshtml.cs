using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.FollowUps;

public class IndexModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;
    private readonly IFollowUpRepository _followUpRepository;

    public IndexModel(BanquetHallDbContext dbContext, IFollowUpRepository followUpRepository)
    {
        _dbContext = dbContext;
        _followUpRepository = followUpRepository;
    }

    public List<LeadFollowUpViewModel> Leads { get; set; } = new();

    public async Task OnGetAsync()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Get guests assigned to this manager (via function) or initiated by them, that have active follow-ups
        var activeFollowUps = await _followUpRepository.GetActiveByManagerIdAsync(managerId);
        var guestIds = activeFollowUps.Select(f => f.GuestId).Distinct().ToList();

        var guests = await _dbContext.Guests
            .Include(g => g.Functions)
            .Include(g => g.FollowUps)
            .Where(g => guestIds.Contains(g.Id))
            .ToListAsync();

        Leads = guests.Select(g =>
        {
            var latestFunction = g.Functions.OrderByDescending(f => f.FunctionDate).FirstOrDefault();
            var latestFollowUp = g.FollowUps.OrderByDescending(f => f.UpdatedAt).FirstOrDefault();
            return new LeadFollowUpViewModel
            {
                GuestId = g.Id,
                FollowUpId = latestFollowUp?.Id ?? 0,
                LeadDate = g.CreatedAt,
                FunctionDate = latestFunction?.FunctionDate,
                Name = g.Name,
                Phone = g.Mobile,
                Status = latestFollowUp?.FollowupStatus ?? g.Status ?? "New",
                LastFollowUpDate = latestFollowUp?.UpdatedAt
            };
        }).OrderByDescending(l => l.LeadDate).ToList();
    }

    public class LeadFollowUpViewModel
    {
        public int GuestId { get; set; }
        public int FollowUpId { get; set; }
        public DateTime LeadDate { get; set; }
        public DateTime? FunctionDate { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? LastFollowUpDate { get; set; }
    }
}
