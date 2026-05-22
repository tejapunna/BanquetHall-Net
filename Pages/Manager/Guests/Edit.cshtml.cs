using System.Security.Claims;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;
using BanquetHall.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Manager.Guests;

public class EditModel : PageModel
{
    private readonly IGuestRepository _guestRepository;
    private readonly IGuestService _guestService;
    private readonly IFollowUpRepository _followUpRepository;

    public EditModel(IGuestRepository guestRepository, IGuestService guestService, IFollowUpRepository followUpRepository)
    {
        _guestRepository = guestRepository;
        _guestService = guestService;
        _followUpRepository = followUpRepository;
    }

    [BindProperty]
    public GuestUpdateDto GuestDto { get; set; } = new();

    public int GuestId { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await _guestRepository.GetByIdAsync(id);
        if (guest == null)
            return NotFound();

        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check edit permission: manager must be initiator OR have active follow-up
        var hasPermission = guest.InitiatedByManagerId == managerId;
        if (!hasPermission)
        {
            var activeFollowUps = await _followUpRepository.GetActiveByManagerIdAsync(managerId);
            hasPermission = activeFollowUps.Any(f => f.GuestId == id);
        }
        
        if (!hasPermission)
            return Forbid();

        GuestId = id;
        GuestDto = new GuestUpdateDto
        {
            Name = guest.Name,
            Mobile = guest.Mobile,
            Email = guest.Email,
            ReferredByName = guest.ReferredByName,
            ReferredByPhone = guest.ReferredByPhone,
            Status = guest.Status
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            GuestId = id;
            return Page();
        }

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _guestService.UpdateGuestAsync(id, GuestDto, userId);
            return RedirectToPage("/Manager/Guests/Index");
        }
        catch (Exception ex)
        {
            GuestId = id;
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
