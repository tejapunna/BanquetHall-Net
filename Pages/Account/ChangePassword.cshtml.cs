using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BanquetHall.Repositories;
using BanquetHall.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Account;

public class ChangePasswordModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public ChangePasswordModel(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "New password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }

    public string UserRole => User.FindFirstValue(ClaimTypes.Role) ?? "Admin";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return Page();
        }

        if (!_authService.VerifyPassword(CurrentPassword, user.PasswordHash))
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            return Page();
        }

        user.PasswordHash = _authService.HashPassword(NewPassword);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        SuccessMessage = "Password changed successfully!";
        return Page();
    }
}
