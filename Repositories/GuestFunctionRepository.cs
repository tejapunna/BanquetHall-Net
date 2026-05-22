using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;
using BanquetHall.Models;

namespace BanquetHall.Repositories;

public class GuestFunctionRepository : Repository<GuestFunction>, IGuestFunctionRepository
{
    public GuestFunctionRepository(BanquetHallDbContext context) : base(context) { }

    public async Task<IEnumerable<GuestFunction>> GetByGuestIdAsync(int guestId)
    {
        return await _dbSet
            .Where(gf => gf.GuestId == guestId)
            .OrderByDescending(gf => gf.FunctionDate)
            .ToListAsync();
    }

    public async Task<GuestFunction?> FindByAadhaarAsync(string aadhaar)
    {
        return await _dbSet
            .Include(gf => gf.Guest)
            .FirstOrDefaultAsync(gf => gf.GuestAadhaar == aadhaar);
    }
}
