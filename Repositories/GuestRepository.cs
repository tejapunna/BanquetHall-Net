using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;
using BanquetHall.Models;

namespace BanquetHall.Repositories;

public class GuestRepository : Repository<Guest>, IGuestRepository
{
    public GuestRepository(BanquetHallDbContext context) : base(context) { }

    public async Task<IEnumerable<Guest>> SearchAsync(string term, int maxResults = 10)
    {
        var lowerTerm = term.ToLower();
        return await _dbSet
            .Where(g => g.Name.ToLower().Contains(lowerTerm) ||
                        g.Mobile.ToLower().Contains(lowerTerm) ||
                        (g.Email != null && g.Email.ToLower().Contains(lowerTerm)))
            .Take(maxResults)
            .ToListAsync();
    }
}
