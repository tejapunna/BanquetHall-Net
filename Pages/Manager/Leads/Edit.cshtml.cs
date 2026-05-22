using System.Security.Claims;
using BanquetHall.Data;
using BanquetHall.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BanquetHall.Pages.Manager.Leads;

public class EditModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;

    public EditModel(BanquetHallDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [BindProperty]
    public LeadEditInput Input { get; set; } = new();

    public List<FunctionName> FunctionNames { get; set; } = new();
    public List<FunctionHall> FunctionHalls { get; set; } = new();
    public List<User> Managers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await _dbContext.Guests.Include(g => g.Functions).FirstOrDefaultAsync(g => g.Id == id);
        if (guest == null) return NotFound();

        var latestFunction = guest.Functions.OrderByDescending(f => f.FunctionDate).FirstOrDefault();

        Input = new LeadEditInput
        {
            GuestId = guest.Id,
            FirstName = guest.FirstName ?? "",
            LastName = guest.LastName ?? "",
            Mobile = guest.Mobile,
            Email = guest.Email,
            Village = guest.Village,
            Mandal = guest.Mandal,
            GuestAadhaar = guest.GuestAadhaar,
            GuestPan = guest.GuestPan,
            ReferredByName = guest.ReferredByName,
            ReferredByPhone = guest.ReferredByPhone,
            Remarks = guest.Remarks,
            FunctionId = latestFunction?.Id ?? 0,
            FunctionDate = latestFunction?.FunctionDate ?? DateTime.Today,
            FunctionNameId = latestFunction?.FunctionNameId ?? 0,
            MealType = latestFunction?.MealType ?? "",
            MealPlan = latestFunction?.MealPlan ?? "",
            NoOfPacks = latestFunction?.NoOfPacks ?? 0,
            GuaranteedPacks = latestFunction?.GuaranteedPacks ?? 0,
            SpecialInstruction = latestFunction?.SpecialInstruction,
            AssignedManagerId = latestFunction?.AssignedManagerId ?? 0,
            FunctionHallId = latestFunction?.FunctionHallId ?? 0
        };

        await LoadDropdowns();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return Page();
        }

        var guest = await _dbContext.Guests.FindAsync(Input.GuestId);
        if (guest == null) return NotFound();

        guest.FirstName = Input.FirstName;
        guest.LastName = Input.LastName;
        guest.Name = $"{Input.FirstName} {Input.LastName}";
        guest.Mobile = Input.Mobile;
        guest.Email = Input.Email;
        guest.Village = Input.Village;
        guest.Mandal = Input.Mandal;
        guest.GuestAadhaar = Input.GuestAadhaar;
        guest.GuestPan = Input.GuestPan;
        guest.ReferredByName = Input.ReferredByName;
        guest.ReferredByPhone = Input.ReferredByPhone;
        guest.Remarks = Input.Remarks;

        if (Input.FunctionId > 0)
        {
            var function = await _dbContext.GuestFunctions.FindAsync(Input.FunctionId);
            if (function != null)
            {
                function.FunctionDate = Input.FunctionDate;
                function.FunctionNameId = Input.FunctionNameId > 0 ? Input.FunctionNameId : null;
                function.MealType = Input.MealType;
                function.MealPlan = Input.MealPlan;
                function.NoOfPacks = Input.NoOfPacks;
                function.GuaranteedPacks = Input.GuaranteedPacks;
                function.SpecialInstruction = Input.SpecialInstruction;
                function.AssignedManagerId = Input.AssignedManagerId > 0 ? Input.AssignedManagerId : null;
                function.FunctionHallId = Input.FunctionHallId > 0 ? Input.FunctionHallId : null;
            }
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToPage("/Manager/Leads/Details", new { id = Input.GuestId });
    }

    private async Task LoadDropdowns()
    {
        FunctionNames = await _dbContext.FunctionNames.Where(f => f.IsActive).OrderBy(f => f.SortOrder).ToListAsync();
        FunctionHalls = await _dbContext.FunctionHalls.Where(f => f.IsActive).OrderBy(f => f.SortOrder).ToListAsync();
        Managers = await _dbContext.Users.Where(u => u.Role == "Manager" && u.IsActive).OrderBy(u => u.FullName).ToListAsync();
    }

    public class LeadEditInput
    {
        public int GuestId { get; set; }
        [Required, MaxLength(100)] public string FirstName { get; set; } = "";
        [Required, MaxLength(100)] public string LastName { get; set; } = "";
        [Required, MaxLength(20)] public string Mobile { get; set; } = "";
        public string? Email { get; set; }
        public string? Village { get; set; }
        public string? Mandal { get; set; }
        public string? GuestAadhaar { get; set; }
        public string? GuestPan { get; set; }
        public string? ReferredByName { get; set; }
        public string? ReferredByPhone { get; set; }
        public string? Remarks { get; set; }
        public int FunctionId { get; set; }
        public DateTime FunctionDate { get; set; }
        public int? FunctionNameId { get; set; }
        public string? MealType { get; set; }
        public string? MealPlan { get; set; }
        public int NoOfPacks { get; set; }
        public int GuaranteedPacks { get; set; }
        public string? SpecialInstruction { get; set; }
        public int? AssignedManagerId { get; set; }
        public int? FunctionHallId { get; set; }
    }
}
