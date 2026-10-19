
using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace SinkDrive.Services;

public record CsvGoogleAccount(string GoogleId, string Email);

public static class CsvService
{
    public static List<CsvGoogleAccount> Read(Stream stream)
    {
        var accounts = new List<CsvGoogleAccount>();

        using var parser = new TextFieldParser(stream, Encoding.UTF8, true);

        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        parser.TrimWhiteSpace = true;

        if (parser.EndOfData)
            throw new InvalidDataException("O CSV está vazio.");

        var headers = parser.ReadFields();

        if (headers is null ||
            !headers.Select(h => h.ToLowerInvariant()).SequenceEqual(
                new[] { "google_id", "email" }))
        {
            throw new InvalidDataException(
                "O cabeçalho deve ser: google_id,email");
        }

        var line = 1;

        while (!parser.EndOfData)
        {
            line++;

            var fields = parser.ReadFields();

            if (fields is null || fields.Length != 2)
                throw new InvalidDataException(
                    $"A linha {line} deve conter dois campos.");

            var googleId = fields[0].Trim();
            var email = fields[1].Trim();

            if (string.IsNullOrWhiteSpace(googleId))
                throw new InvalidDataException(
                    $"Google ID vazio na linha {line}.");

            if (!System.Net.Mail.MailAddress.TryCreate(email, out var address)
                || address.Address != email)
            {
                throw new InvalidDataException(
                    $"E-mail inválido na linha {line}.");
            }

            accounts.Add(new CsvGoogleAccount(googleId, email));
        }

        return accounts;
    }

    public static byte[] Write(IEnumerable<CsvGoogleAccount> accounts)
    {
        var csv = new StringBuilder();
        csv.AppendLine("google_id,email");

        foreach (var account in accounts)
        {
            csv.Append(Escape(account.GoogleId));
            csv.Append(',');
            csv.AppendLine(Escape(account.Email));
        }

        return new UTF8Encoding(true).GetBytes(csv.ToString());
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') &&
            !value.Contains('"') &&
            !value.Contains('\n') &&
            !value.Contains('\r'))
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
