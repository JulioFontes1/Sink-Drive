using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinkDrive.Data;
using SinkDrive.Models;
using SinkDrive.Services;

namespace SinkDrive.Controllers;

[Authorize]
[Route("auth/google")]
public class GoogleAuthController : Controller
{
    private const string StateCookie = "google_oauth_state";

    private readonly GoogleOAuthService _googleOAuth;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public GoogleAuthController(
        GoogleOAuthService googleOAuth,
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _googleOAuth = googleOAuth;
        _db = db;
        _userManager = userManager;
    }

    private string BuildRedirectUri() =>
        $"{Request.Scheme}://{Request.Host}/auth/google/callback";

    [HttpGet("connect")]
    public IActionResult Connect()
    {
        var state = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        Response.Cookies.Append(StateCookie, state, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10)
        });

        var url = _googleOAuth.BuildAuthorizationUrl(BuildRedirectUri(), state);

        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string? code, string? state, string? error)
    {
        var expectedState = Request.Cookies[StateCookie];
        Response.Cookies.Delete(StateCookie);

        if (!string.IsNullOrEmpty(error))
            return BadRequest($"O Google retornou um erro: {error}");

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state) || state != expectedState)
            return BadRequest("Requisição inválida (state incorreto).");

        var userId = _userManager.GetUserId(User)!;
        var identity = await _googleOAuth.ExchangeCodeAsync(code, BuildRedirectUri());

        var account = await _db.GoogleAccounts
            .FirstOrDefaultAsync(a => a.GoogleId == identity.GoogleId);

        if (account is not null && account.UserId != userId)
            return Conflict("Essa conta Google já está vinculada a outro usuário.");

        if (account is null)
        {
            account = new GoogleAccount
            {
                GoogleId = identity.GoogleId,
                Email = identity.Email,
                UserId = userId
            };
            _db.GoogleAccounts.Add(account);
        }

        account.Email = identity.Email;
        if (identity.RefreshToken is not null)
            account.RefreshToken = identity.RefreshToken;

        await _db.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}