using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Repositories;
using BanquetHall.Services;

namespace BanquetHall.Pages.Admin.Users;

public class EditModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public EditModel(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    [BindProperty]
    public EditUserInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        Input = new EditUserInput
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userRepository.GetByIdAsync(Input.Id);
        if (user == null)
            return NotFound();

        // Prevent self-deactivation
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (user.Id == currentUserId && !Input.IsActive)
        {
            ModelState.AddModelError(string.Empty, "You cannot deactivate your own account.");
            return Page();
        }

        user.FullName = Input.FullName;
        user.Role = Input.Role;
        user.IsActive = Input.IsActive;

        if (!string.IsNullOrWhiteSpace(Input.NewPassword))
        {
            user.PasswordHash = _authService.HashPassword(Input.NewPassword);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return RedirectToPage("/Admin/Users/Index");
    }

    public class EditUserInput
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [MinLength(6)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }
    }
}
