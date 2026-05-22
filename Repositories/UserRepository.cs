using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;
using BanquetHall.Models;

namespace BanquetHall.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(BanquetHallDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(string role)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }
}
