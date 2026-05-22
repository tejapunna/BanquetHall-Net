using BanquetHall.Models;

namespace BanquetHall.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetByGuestIdAsync(int guestId);
    Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to);
}
