using System.ComponentModel.DataAnnotations;

namespace BanquetHall.ViewModels;

public class GuestUpdateDto
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(150), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(150)]
    public string? ReferredByName { get; set; }

    [MaxLength(20)]
    public string? ReferredByPhone { get; set; }

    public string? Status { get; set; }
}
