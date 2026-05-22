using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanquetHall.Models;

[Table("Payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }
    public int GuestId { get; set; }
    public int FunctionId { get; set; }

    [MaxLength(50)]
    public string PaymentType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public int NoOfPacks { get; set; }
    public int GuaranteedPacks { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerPack { get; set; }

    public bool AdvancePaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdvanceAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Guest Guest { get; set; } = null!;
    public GuestFunction Function { get; set; } = null!;
}
