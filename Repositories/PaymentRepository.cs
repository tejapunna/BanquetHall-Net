using Microsoft.EntityFrameworkCore;
using BanquetHall.Data;
using BanquetHall.Models;

namespace BanquetHall.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(BanquetHallDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetByGuestIdAsync(int guestId)
    {
        return await _dbSet
            .Where(p => p.GuestId == guestId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .ToListAsync();
    }
}
