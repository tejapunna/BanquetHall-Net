namespace BanquetHall.ViewModels;

public class FollowUpMonitorDto
{
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public int FollowUpsUpdatedToday { get; set; }
    public List<ContactedGuestDto> ContactedGuests { get; set; } = new();
    public List<UpcomingFollowUpDto> UpcomingFollowUps { get; set; } = new();
}

public class ContactedGuestDto
{
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string FollowupStatus { get; set; } = string.Empty;
}

public class UpcomingFollowUpDto
{
    public int FollowUpId { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public DateTime FollowupDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
