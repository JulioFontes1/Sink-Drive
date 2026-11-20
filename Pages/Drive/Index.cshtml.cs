using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SinkDrive.Data;
using SinkDrive.Models;
using SinkDrive.Services;

namespace SinkDrive.Pages.Drive;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly DriveBrowserService _drive;

    public IndexModel(ApplicationDbContext db, DriveBrowserService drive)
    {
        _db = db;
        _drive = drive;
    }

    [BindProperty(SupportsGet = true)] public int? AccountId { get; set; }
    [BindProperty(SupportsGet = true)] public string? FolderId { get; set; }

    public List<GoogleAccount> Accounts { get; private set; } = new();
    public List<DriveItemVm> Items { get; private set; } = new();
    public List<BreadcrumbVm> Breadcrumb { get; private set; } = new();
    public Dictionary<int, string> Errors { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Accounts = await _db.GoogleAccounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Email)
            .ToListAsync();

        // Uma pasta só existe dentro de uma conta; sem conta, volta para a raiz.
        if (!string.IsNullOrEmpty(FolderId) && AccountId is null)
            return RedirectToPage(new { });

        var selected = AccountId is null
            ? Accounts
            : Accounts.Where(a => a.Id == AccountId).ToList();

        // A conta pedida não existe ou não pertence ao usuário.
        if (AccountId is not null && selected.Count == 0)
            return RedirectToPage(new { });

        // Consulta cada conta em paralelo; se uma falhar, as outras continuam.
        var tasks = selected.Select(async acc =>
        {
            try
            {
                if (string.IsNullOrEmpty(acc.RefreshToken))
                    throw new InvalidOperationException("Conta sem token de acesso.");

                var items = await _drive.ListItemsAsync(acc, FolderId);
                return (acc, items, error: (string?)null);
            }
            catch (Exception)
            {
                return (acc, items: new List<DriveItemVm>(),
                        error: "Não foi possível acessar esta conta. O acesso pode ter sido revogado; tente reconectá-la.");
            }
        }).ToList();

        foreach (var (acc, items, error) in await Task.WhenAll(tasks))
        {
            Items.AddRange(items);
            if (error != null) Errors[acc.Id] = error;
        }

        // Pastas primeiro, depois por nome.
        Items = Items
            .OrderByDescending(i => i.IsFolder)
            .ThenBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        // O caminho só faz sentido dentro de uma pasta de uma única conta.
        if (!string.IsNullOrEmpty(FolderId) && selected.Count == 1)
        {
            try { Breadcrumb = await _drive.GetBreadcrumbAsync(selected[0], FolderId); }
            catch { /* o caminho é opcional */ }
        }

        return Page();
    }

    public static string FormatSize(long? bytes)
    {
        if (bytes is null) return "—";

        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes.Value;
        var u = 0;

        while (size >= 1024 && u < units.Length - 1)
        {
            size /= 1024;
            u++;
        }

        return $"{size:0.#} {units[u]}";
    }

    public static string IconFor(DriveItemVm item)
    {
        if (item.IsFolder) return "📁";

        var m = item.MimeType;
        if (m.StartsWith("image/")) return "🖼️";
        if (m.StartsWith("video/")) return "🎞️";
        if (m.StartsWith("audio/")) return "🎵";
        if (m == "application/pdf") return "📕";
        if (m.Contains("spreadsheet") || m.Contains("excel")) return "📊";
        if (m.Contains("presentation") || m.Contains("powerpoint")) return "📽️";
        if (m.Contains("document") || m.Contains("word") || m.StartsWith("text/")) return "📄";
        if (m.Contains("zip") || m.Contains("compressed")) return "🗜️";

        return "📎";
    }
}