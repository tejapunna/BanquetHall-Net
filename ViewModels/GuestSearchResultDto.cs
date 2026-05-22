namespace BanquetHall.ViewModels;

public class GuestSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Status { get; set; }
    public string? CurrentManager { get; set; }
    public string? InitiatedByManager { get; set; }
}
