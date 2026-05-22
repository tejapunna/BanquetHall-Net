using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BanquetHall.Pages.Receptionist;

public class IndexModel : PageModel
{
    public string ReceptionistName { get; set; } = string.Empty;

    public void OnGet()
    {
        ReceptionistName = User.FindFirst("FullName")?.Value ?? "Receptionist";
    }
}
