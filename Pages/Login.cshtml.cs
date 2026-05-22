using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Services;
using BanquetHall.ViewModels;

namespace BanquetHall.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _authService.ValidateCredentialsAsync(Input.Username, Input.Password);
        if (user == null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var principal = await _authService.CreateClaimsPrincipalAsync(user);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Redirect to role-appropriate dashboard
        return user.Role switch
        {
            "Admin" => RedirectToPage("/Admin/Index"),
            "Manager" => RedirectToPage("/Manager/Index"),
            "Receptionist" => RedirectToPage("/Receptionist/Index"),
            _ => RedirectToPage("/Index")
        };
    }
}
