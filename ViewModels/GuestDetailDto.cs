namespace BanquetHall.ViewModels;

public class GuestDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ReferredByName { get; set; }
    public string? ReferredByPhone { get; set; }
    public string? Status { get; set; }
    public string? InitiatedByManager { get; set; }
    public string? CurrentFollowUpManager { get; set; }
    public List<FunctionHistoryDto> Functions { get; set; } = new();
}

public class FunctionHistoryDto
{
    public int Id { get; set; }
    public DateTime FunctionDate { get; set; }
    public string FunctionType { get; set; } = string.Empty;
    public string MealPlan { get; set; } = string.Empty;
    public string? GuestAddress { get; set; }
    public string? GuestAadhaar { get; set; }
    public string? InitiatedBy { get; set; }
}
