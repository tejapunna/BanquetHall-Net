using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Booking;

public class DetailsModel : PageModel
{
    private readonly IGuestFunctionRepository _guestFunctionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFollowUpRepository _followUpRepository;
    private readonly IGuestRepository _guestRepository;

    public DetailsModel(
        IGuestFunctionRepository guestFunctionRepository,
        IPaymentRepository paymentRepository,
        IFollowUpRepository followUpRepository,
        IGuestRepository guestRepository)
    {
        _guestFunctionRepository = guestFunctionRepository;
        _paymentRepository = paymentRepository;
        _followUpRepository = followUpRepository;
        _guestRepository = guestRepository;
    }

    public GuestFunction Booking { get; set; } = null!;
    public Guest Guest { get; set; } = null!;
    public Payment? Payment { get; set; }
    public FollowUp? LatestFollowUp { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var booking = await _guestFunctionRepository.Query()
            .Include(gf => gf.Guest)
            .FirstOrDefaultAsync(gf => gf.Id == id);

        if (booking == null)
            return NotFound();

        Booking = booking;
        Guest = booking.Guest;

        Payment = await _paymentRepository.Query()
            .Where(p => p.GuestId == booking.GuestId && p.FunctionId == booking.Id)
            .FirstOrDefaultAsync();

        LatestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(booking.GuestId);

        return Page();
    }
}
