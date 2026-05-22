using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanquetHall.Models;

[Table("FollowUps")]
public class FollowUp
{
    [Key]
    public int Id { get; set; }
    public int GuestId { get; set; }
    public int ManagerId { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(50)]
    public string FollowupStatus { get; set; } = string.Empty;

    public DateTime FollowupDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Guest Guest { get; set; } = null!;
    public User Manager { get; set; } = null!;
}
