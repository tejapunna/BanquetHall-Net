using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Services;
using BanquetHall.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Manager.Booking;

[IgnoreAntiforgeryToken]
[Authorize(Roles = "Manager,Receptionist")]
public class WizardModel : PageModel
{
    private readonly IGuestService _guestService;
    private readonly IFunctionService _functionService;
    private readonly BanquetHallDbContext _dbContext;

    public WizardModel(
        IGuestService guestService,
        IFunctionService functionService,
        BanquetHallDbContext dbContext)
    {
        _guestService = guestService;
        _functionService = functionService;
        _dbContext = dbContext;
    }

    public void OnGet()
    {
    }

    // Step 1: Create Guest
    public async Task<IActionResult> OnPostStep1Async([FromBody] GuestCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return new JsonResult(new { success = false, errors });
            }

            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var guest = await _guestService.CreateGuestAsync(dto, managerId);
            return new JsonResult(new { success = true, data = new { guestId = guest.Id } });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, errors = new { General = new[] { ex.Message } } });
        }
    }

    // Step 2: Create Function
    public async Task<IActionResult> OnPostStep2Async([FromBody] FunctionCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
                return new JsonResult(new { success = false, errors });
            }

            var managerName = User.FindFirst("FullName")?.Value ?? "Manager";
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var function = await _functionService.CreateFunctionAsync(dto, managerName, managerId);
            return new JsonResult(new { success = true, data = new { functionId = function.Id } });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, errors = new { General = new[] { ex.Message } } });
        }
    }

    // AJAX: Get active FunctionNames for dropdown
    public async Task<IActionResult> OnGetFunctionNamesAsync()
    {
        var items = await _dbContext.FunctionNames
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .Select(f => new { f.Id, f.Name })
            .ToListAsync();
        return new JsonResult(items);
    }

    // AJAX: Get active FunctionHalls for dropdown
    public async Task<IActionResult> OnGetFunctionHallsAsync()
    {
        var items = await _dbContext.FunctionHalls
            .Where(f => f.IsActive)
            .OrderBy(f => f.SortOrder)
            .Select(f => new { f.Id, f.Name })
            .ToListAsync();
        return new JsonResult(items);
    }

    // AJAX: Get managers for dropdown
    public async Task<IActionResult> OnGetManagersAsync()
    {
        var managers = await _dbContext.Users
            .Where(u => u.Role == "Manager" && u.IsActive)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();
        return new JsonResult(managers);
    }
}
