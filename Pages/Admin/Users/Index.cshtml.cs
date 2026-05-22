using Microsoft.AspNetCore.Mvc.RazorPages;
using BanquetHall.Models;
using BanquetHall.Repositories;

namespace BanquetHall.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IUserRepository _userRepository;

    public IndexModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();

    public async Task OnGetAsync()
    {
        Users = await _userRepository.GetAllAsync();
    }
}
