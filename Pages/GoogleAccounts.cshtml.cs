using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SinkDrive.Data;
using SinkDrive.Models;

namespace SinkDrive.Pages;

[Authorize]
public class GoogleAccounts : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public GoogleAccounts(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<GoogleAccount> Accounts { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User);

        Accounts = await _db.GoogleAccounts
            .Where(account => account.UserId == userId)
            .OrderBy(account => account.Email)
            .ToListAsync();
    }
}