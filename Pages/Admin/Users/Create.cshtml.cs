using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Models;
using BanquetHall.Repositories;
using BanquetHall.Services;

namespace BanquetHall.Pages.Admin.Users;

public class CreateModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public CreateModel(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    [BindProperty]
    public CreateUserInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (await _userRepository.UsernameExistsAsync(Input.Username))
        {
            ModelState.AddModelError("Input.Username", "Username already exists.");
            return Page();
        }

        var user = new User
        {
            Username = Input.Username,
            FullName = Input.FullName,
            Role = Input.Role,
            PasswordHash = _authService.HashPassword(Input.Password),
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return RedirectToPage("/Admin/Users/Index");
    }

    public class CreateUserInput
    {
        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
