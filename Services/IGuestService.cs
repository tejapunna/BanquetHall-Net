using BanquetHall.Models;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public interface IGuestService
{
    Task<Guest> CreateGuestAsync(GuestCreateDto dto, int managerId);
    Task<Guest> UpdateGuestAsync(int guestId, GuestUpdateDto dto, int userId);
    Task<IEnumerable<GuestSearchResultDto>> SearchGuestsAsync(string term);
    Task<GuestDetailDto?> GetGuestDetailAsync(int guestId, string userRole);
}
