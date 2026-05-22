using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanquetHall.Models;

[Table("GuestFunctions")]
public class GuestFunction
{
    [Key]
    public int Id { get; set; }
    public int GuestId { get; set; }
    public DateTime FunctionDate { get; set; }

    [MaxLength(100)]
    public string FunctionType { get; set; } = string.Empty;

    public int? FunctionNameId { get; set; }

    [MaxLength(150)]
    public string MealPlan { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? MealType { get; set; }

    public int NoOfPacks { get; set; }
    public int GuaranteedPacks { get; set; }

    public string? SpecialInstruction { get; set; }

    public int? AssignedManagerId { get; set; }
    public int? FunctionHallId { get; set; }

    [MaxLength(150)]
    public string? InitiatedBy { get; set; }

    public string? GuestAddress { get; set; }

    [MaxLength(20)]
    public string? GuestAadhaar { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public Guest Guest { get; set; } = null!;
    public FunctionName? FunctionName { get; set; }
    public FunctionHall? FunctionHall { get; set; }
    public User? AssignedManager { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
