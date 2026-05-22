using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanquetHall.Models;

[Table("Guests")]
public class Guest
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required, MaxLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? Village { get; set; }

    [MaxLength(150)]
    public string? Mandal { get; set; }

    [MaxLength(20)]
    public string? GuestAadhaar { get; set; }

    [MaxLength(20)]
    public string? GuestPan { get; set; }

    public string? Remarks { get; set; }

    [MaxLength(150)]
    public string? ReferredByName { get; set; }

    [MaxLength(20)]
    public string? ReferredByPhone { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    public int? InitiatedByManagerId { get; set; }

    // Navigation
    public ICollection<GuestFunction> Functions { get; set; } = new List<GuestFunction>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}
