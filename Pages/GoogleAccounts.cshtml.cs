using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IHttpClientFactory _httpClientFactory;   

    public GoogleAccounts(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IHttpClientFactory httpClientFactory)                
    {
        _db = db;
        _userManager = userManager;
        _httpClientFactory = httpClientFactory;              
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

    public async Task<IActionResult> OnPostDisconnectAsync(int id)
    {
        var currentUserId = _userManager.GetUserId(User);

        var googleAccount = await _db.GoogleAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == currentUserId);

        if (googleAccount == null) return NotFound();

        if (!string.IsNullOrEmpty(googleAccount.RefreshToken))
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();   // novo
                await httpClient.PostAsync(
                    "https://oauth2.googleapis.com/revoke",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["token"] = googleAccount.RefreshToken
                    }));
            }
            catch (HttpRequestException)
            {
                // falha de rede: segue para a remoção local
            }
        }

        _db.GoogleAccounts.Remove(googleAccount);
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }
}