using BanquetHall.Data;
using BanquetHall.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BanquetHall.Pages.Admin.Settings;

[IgnoreAntiforgeryToken]
public class FunctionNamesModel : PageModel
{
    private readonly BanquetHallDbContext _dbContext;

    public FunctionNamesModel(BanquetHallDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<FunctionName> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _dbContext.FunctionNames.OrderBy(f => f.SortOrder).ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync([FromBody] FunctionNameDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return new JsonResult(new { success = false, error = "Name is required." });

        var item = new FunctionName
        {
            Name = dto.Name,
            SortOrder = dto.SortOrder,
            IsActive = true
        };

        _dbContext.FunctionNames.Add(item);
        await _dbContext.SaveChangesAsync();
        return new JsonResult(new { success = true, data = new { item.Id, item.Name, item.SortOrder, item.IsActive } });
    }

    public async Task<IActionResult> OnPostUpdateAsync([FromBody] FunctionNameDto dto)
    {
        var item = await _dbContext.FunctionNames.FindAsync(dto.Id);
        if (item == null)
            return new JsonResult(new { success = false, error = "Not found." });

        item.Name = dto.Name;
        item.SortOrder = dto.SortOrder;
        await _dbContext.SaveChangesAsync();
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync([FromBody] ToggleDto dto)
    {
        var item = await _dbContext.FunctionNames.FindAsync(dto.Id);
        if (item == null)
            return new JsonResult(new { success = false, error = "Not found." });

        item.IsActive = !item.IsActive;
        await _dbContext.SaveChangesAsync();
        return new JsonResult(new { success = true, isActive = item.IsActive });
    }

    public class FunctionNameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class ToggleDto
    {
        public int Id { get; set; }
    }
}
