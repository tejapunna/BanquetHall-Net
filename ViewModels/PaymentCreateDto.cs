using System.ComponentModel.DataAnnotations;

namespace BanquetHall.ViewModels;

public class PaymentCreateDto
{
    [Required]
    public int GuestId { get; set; }

    [Required]
    public int FunctionId { get; set; }

    [Required]
    public string PaymentType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int NoOfPacks { get; set; }

    [Range(1, int.MaxValue)]
    public int GuaranteedPacks { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PricePerPack { get; set; }

    public bool AdvancePaid { get; set; }

    [Range(0, double.MaxValue)]
    public decimal AdvanceAmount { get; set; }
}
