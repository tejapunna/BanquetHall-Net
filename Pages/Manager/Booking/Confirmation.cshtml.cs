using BanquetHall.Models;
using BanquetHall.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Booking;

public class ConfirmationModel : PageModel
{
    private readonly IGuestRepository _guestRepository;
    private readonly IGuestFunctionRepository _guestFunctionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IFollowUpRepository _followUpRepository;

    public ConfirmationModel(
        IGuestRepository guestRepository,
        IGuestFunctionRepository guestFunctionRepository,
        IPaymentRepository paymentRepository,
        IFollowUpRepository followUpRepository)
    {
        _guestRepository = guestRepository;
        _guestFunctionRepository = guestFunctionRepository;
        _paymentRepository = paymentRepository;
        _followUpRepository = followUpRepository;
    }

    public Guest Guest { get; set; } = null!;
    public GuestFunction Function { get; set; } = null!;
    public Payment? Payment { get; set; }
    public FollowUp? LatestFollowUp { get; set; }

    public async Task<IActionResult> OnGetAsync(int guestId, int functionId, int? paymentId)
    {
        var guest = await _guestRepository.GetByIdAsync(guestId);
        if (guest == null)
            return NotFound();

        Guest = guest;

        var function = await _guestFunctionRepository.GetByIdAsync(functionId);
        if (function == null)
            return NotFound();

        Function = function;

        if (paymentId.HasValue)
        {
            Payment = await _paymentRepository.GetByIdAsync(paymentId.Value);
        }
        else
        {
            // Get latest payment for this guest/function
            Payment = await _paymentRepository.Query()
                .Where(p => p.GuestId == guestId && p.FunctionId == functionId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        LatestFollowUp = await _followUpRepository.GetLatestByGuestIdAsync(guestId);

        return Page();
    }
}
