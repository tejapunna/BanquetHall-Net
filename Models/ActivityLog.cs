using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanquetHall.Models;

[Table("ActivityLogs")]
public class ActivityLog
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }

    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
