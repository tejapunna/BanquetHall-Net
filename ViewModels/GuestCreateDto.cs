using System.ComponentModel.DataAnnotations;

namespace BanquetHall.ViewModels;

public class GuestCreateDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(150), EmailAddress]
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
}
