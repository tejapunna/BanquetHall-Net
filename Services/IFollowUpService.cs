using BanquetHall.Models;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public interface IFollowUpService
{
    Task<FollowUp> CreateFollowUpAsync(FollowUpCreateDto dto, int managerId, int userId);
    Task<FollowUp> UpdateFollowUpAsync(int followUpId, FollowUpUpdateDto dto, int userId);
    Task<int> TransferLeadsAsync(int sourceManagerId, int targetManagerId, int adminUserId);
}
