using BanquetHall.Models;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public interface IFunctionService
{
    Task<GuestFunction> CreateFunctionAsync(FunctionCreateDto dto, string managerName, int userId);
    Task<IEnumerable<GuestFunction>> GetFunctionsByGuestAsync(int guestId);
}
