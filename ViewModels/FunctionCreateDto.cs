using System.ComponentModel.DataAnnotations;

namespace BanquetHall.ViewModels;

public class FunctionCreateDto
{
    [Required]
    public int GuestId { get; set; }

    [Required]
    public DateTime FunctionDate { get; set; }

    [Required]
    public int FunctionNameId { get; set; }

    [Required]
    public string MealType { get; set; } = string.Empty; // Veg or Non-Veg

    [Required]
    public string MealPlan { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, HiTea

    public int NoOfPacks { get; set; }
    public int GuaranteedPacks { get; set; }

    public string? SpecialInstruction { get; set; }

    [Required]
    public int AssignedManagerId { get; set; }

    [Required]
    public int FunctionHallId { get; set; }
}
