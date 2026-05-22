using BanquetHall.Models;
using BanquetHall.ViewModels;

namespace BanquetHall.Services;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(PaymentCreateDto dto, int userId);
    Task<PaymentSummaryDto> CalculatePaymentAsync(int guaranteedPacks, decimal pricePerPack, decimal advanceAmount);
    Task<Payment> CollectRemainingPaymentAsync(int paymentId, decimal amountCollected, string paymentType, int userId);
}
