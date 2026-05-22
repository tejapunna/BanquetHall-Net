using System.ComponentModel.DataAnnotations;

namespace BanquetHall.ViewModels;

public class FollowUpUpdateDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [Required]
    public string FollowupStatus { get; set; } = string.Empty;

    [Required]
    public DateTime FollowupDate { get; set; }

    [MaxLength(2000)]
    public string? Remarks { get; set; }
}
