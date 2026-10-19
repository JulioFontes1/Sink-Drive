using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinkDrive.Services;

namespace SinkDrive.Pages;

public class CsvDemoModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public CsvDemoModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public List<CsvGoogleAccount> Accounts { get; private set; } = [];
    public string? ErrorMessage { get; private set; }
    public bool FileRead { get; private set; }

    public void OnGet()
    {
    }

    public IActionResult OnPostRead()
    {
        try
        {
            string path = Path.Combine(
                _environment.ContentRootPath,
                "Data", "Csv", "google_accounts.csv");

            using var stream = System.IO.File.OpenRead(path);

            Accounts = CsvService.Read(stream);
            FileRead = true;
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is InvalidDataException)
        {
            ErrorMessage = $"Não foi possível ler o CSV: {ex.Message}";
        }

        return Page();
    }

    public IActionResult OnPostDownload()
    {
        try
        {
            string path = Path.Combine(
                _environment.ContentRootPath,
                "Data", "Csv", "google_accounts.csv");

            using var stream = System.IO.File.OpenRead(path);

            var accounts = CsvService.Read(stream);
            byte[] csv = CsvService.Write(accounts);

            return File(
                csv,
                "text/csv; charset=utf-8",
                "google_accounts_gerado.csv");
        }
        catch (Exception ex) when (
            ex is IOException ||
            ex is UnauthorizedAccessException ||
            ex is InvalidDataException)
        {
            ErrorMessage = $"Não foi possível gerar o CSV: {ex.Message}";
            return Page();
        }
    }
}