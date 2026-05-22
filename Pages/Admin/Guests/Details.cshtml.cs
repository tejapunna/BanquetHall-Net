using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;
using BanquetHall.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Guests;

public class DetailsModel : PageModel
{
    private readonly IGuestService _guestService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFollowUpRepository _followUpRepository;

    public DetailsModel(
        IGuestService guestService,
        IPaymentRepository paymentRepository,
        IFollowUpRepository followUpRepository)
    {
        _guestService = guestService;
        _paymentRepository = paymentRepository;
        _followUpRepository = followUpRepository;
    }

    public GuestDetailDto GuestDetail { get; set; } = null!;
    public List<Payment> Payments { get; set; } = new();
    public List<FollowUp> FollowUps { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var detail = await _guestService.GetGuestDetailAsync(id, "Admin");
        if (detail == null)
            return NotFound();

        GuestDetail = detail;

        Payments = (await _paymentRepository.GetByGuestIdAsync(id)).ToList();

        FollowUps = await _followUpRepository.Query()
            .Include(f => f.Manager)
            .Where(f => f.GuestId == id)
            .OrderByDescending(f => f.FollowupDate)
            .ToListAsync();

        return Page();
    }
}
