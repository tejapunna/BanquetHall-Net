using BanquetHall.Models;

namespace BanquetHall.Repositories;

public interface IFollowUpRepository : IRepository<FollowUp>
{
    Task<IEnumerable<FollowUp>> GetByManagerIdAsync(int managerId);
    Task<IEnumerable<FollowUp>> GetActiveByManagerIdAsync(int managerId);
    Task<FollowUp?> GetLatestByGuestIdAsync(int guestId);
}
