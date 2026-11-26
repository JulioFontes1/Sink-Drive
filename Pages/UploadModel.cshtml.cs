using System.Security.Claims;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SinkDrive.Data;
using SinkDrive.Services;

namespace SinkDrive.Pages;

[Authorize]
[RequestSizeLimit(MaxBytes)]
[RequestFormLimits(MultipartBodyLengthLimit = MaxBytes)]
public class UploadModel : PageModel
{
    private const long MaxBytes = 100L * 1024 * 1024;
    
    private readonly ApplicationDbContext _db;
    private readonly GoogleDriveUploadService _uploader;

    public UploadModel(ApplicationDbContext db, GoogleDriveUploadService uploader)
    {
        _db = db;
        _uploader = uploader;
    }

    public List<SelectListItem> Accounts { get; set; } = new();
    
    [BindProperty(SupportsGet = true)]
    public string? FolderId { get; set; }
    
    [BindProperty]
    public int GoogleAccountId { get; set; }
    [BindProperty]
    public IFormFile? UploadedFile { get; set; }
    
    [TempData]
    public string? Message { get; set; }
    [TempData]
    public string? Error { get; set; }

    public async Task OnGetAsync() => await LoadAccountsAsync();

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Console.WriteLine($"Files={Request.Form.Files.Count} Names=[{string.Join(",", Request.Form.Files.Select(f => f.Name))}] Bound={(UploadedFile is null ? "null" : UploadedFile.Length.ToString())}");
        await LoadAccountsAsync();

        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ModelState.AddModelError(nameof(UploadedFile), "Selecione um arquivo!");
            return Page();
        }

        if (UploadedFile.Length > MaxBytes)
        {
            ModelState.AddModelError(nameof(UploadedFile), "Arquivo maior que 100MB.");
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var account = await _db.GoogleAccounts
            .FirstOrDefaultAsync(a => a.Id == GoogleAccountId && a.UserId == userId, ct);

        if (account is null)
        {
            ModelState.AddModelError(nameof(GoogleAccountId), "Conta inválida.");
        }

        try
        {
            await using var stream = UploadedFile.OpenReadStream();
            var uploaded = await _uploader.UploadAsync(
                account, stream, Path.GetFileName(UploadedFile.FileName),
                string.IsNullOrWhiteSpace(UploadedFile.ContentType) ? "application/octet-stream" : UploadedFile.ContentType,
                FolderId, ct);
            
            Message = $"\"{uploaded.Name}\" enviado para {account.Email}.";
        }
        catch (TokenResponseException)
        {
            Error = $"O acesso de {account.Email} foi revogado ou expirou. Reconecte a conta.";
        }
        catch (Exception)
        {
            Error = "Não foi possível enviar o arquivo. Tente novamente.";
        }
        return Page();
    }

    public async Task<IActionResult> OnGetFoldersAsync(int accountId, string? parentId, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var account = await _db.GoogleAccounts
            .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, ct);
        
        if (account is null) return NotFound();

        try
        {
            var folder = await _uploader.ListFolderAsync(account, parentId, ct);
            return new JsonResult(folder.Select(f => new { id = f.Id, name = f.Name }));
        }
        catch(Exception ex)
        {
            Console.WriteLine("Falhou: " + ex);
            return StatusCode(502);
        }
    }

    private async Task LoadAccountsAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Accounts = await _db.GoogleAccounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Email)
            .Select(a => new SelectListItem(a.Email, a.Id.ToString()))
            .ToListAsync();
    }
}

