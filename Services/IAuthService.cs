using System.Security.Claims;
using BanquetHall.Models;

namespace BanquetHall.Services;

public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string username, string password);
    Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
