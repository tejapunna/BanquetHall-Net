using BanquetHall.Models;

namespace BanquetHall.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> UsernameExistsAsync(string username);
    Task<IEnumerable<User>> GetByRoleAsync(string role);
}
