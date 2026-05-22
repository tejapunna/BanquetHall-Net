using BanquetHall.Models;

namespace BanquetHall.Repositories;

public interface IGuestRepository : IRepository<Guest>
{
    Task<IEnumerable<Guest>> SearchAsync(string term, int maxResults = 10);
}
