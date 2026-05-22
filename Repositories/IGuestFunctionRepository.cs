using BanquetHall.Models;

namespace BanquetHall.Repositories;

public interface IGuestFunctionRepository : IRepository<GuestFunction>
{
    Task<IEnumerable<GuestFunction>> GetByGuestIdAsync(int guestId);
    Task<GuestFunction?> FindByAadhaarAsync(string aadhaar);
}
